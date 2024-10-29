using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Byrone.Xenia.Internal;
using Byrone.Xenia.Internal.Extensions;
using Byrone.Xenia.Utilities;
using JetBrains.Annotations;

namespace Byrone.Xenia
{
	[StructLayout(LayoutKind.Sequential)]
	public readonly ref struct Multipart
	{
		private readonly System.ReadOnlySpan<byte> data;
		private readonly System.ReadOnlySpan<byte> boundary;

		public bool IsValid =>
			!this.data.IsEmpty && !this.boundary.IsEmpty;

		private Multipart(System.ReadOnlySpan<byte> boundary, System.ReadOnlySpan<byte> data)
		{
			this.data = data;
			this.boundary = boundary;
		}

		// @todo Can be multiple items with the same name, handle somehow
		public bool TryGet(scoped System.ReadOnlySpan<byte> name, [MustDisposeResource] out Item item)
		{
			var enumerator = new SpanSplitEnumerator(this.data, this.boundary);
			enumerator.MoveNext(); // First slice will simply be: '--'

			foreach (var part in enumerator)
			{
				// If we've reached a part that's only '--' we reached the end of the multipart data.
				if (System.MemoryExtensions.SequenceEqual(part, "--"u8))
				{
					break;
				}

				var multipartItem = Multipart.ParseItem(part);

				if (!System.MemoryExtensions.SequenceEqual(name, multipartItem.Name))
				{
					multipartItem.Dispose();
					continue;
				}

				item = multipartItem;
				return true;
			}

			item = default;
			return false;
		}

		[MustDisposeResource]
		private static Item ParseItem(System.ReadOnlySpan<byte> data)
		{
			while (System.MemoryExtensions.StartsWith(data, Characters.HttpSeparator))
			{
				data = data.SliceUnsafe(Characters.HttpSeparator.Length);
			}

			var enumerator = new SpanSplitEnumerator(data, Characters.HttpSeparator);

			// It's possible that we rent 1/2 too many items, but this shouldn't matter (most of the time)...
			// ...as the array size gets rounded to a ^2 number anyway.
			var headers = new RentedArray<KeyValuePair<Unmanaged, Unmanaged>>(enumerator.Count);
			var i = 0;

			System.ReadOnlySpan<byte> content = default;

			foreach (var row in enumerator)
			{
				if (System.MemoryExtensions.SequenceEqual(row, "--"u8))
				{
					break;
				}

				var header = Multipart.ParseHeader(row);

				if (!header.Key.IsEmpty)
				{
					headers[i++] = header;
				}
				else
				{
					content = row;
				}
			}

			return new Item(content, headers);
		}

		private static KeyValuePair<Unmanaged, Unmanaged> ParseHeader(scoped System.ReadOnlySpan<byte> header)
		{
			// https://datatracker.ietf.org/doc/html/rfc7578#section-4.8 declares that these are the only allowed headers.
			if (!System.MemoryExtensions.StartsWith(header, "Content-Disposition"u8) &&
				!System.MemoryExtensions.StartsWith(header, "Content-Type"u8) &&
				!System.MemoryExtensions.StartsWith(header, "Content-Transfer-Encoding"u8))
			{
				return default;
			}

			var separator = System.MemoryExtensions.IndexOf(header, Characters.Colon);

			// Not a valid header
			if (separator == -1)
			{
				return default;
			}

			var key = header.SliceUnsafe(0, separator);
			var value = header.SliceUnsafe(separator + 1);

			// Trim possible leading space
			if (value[0] == Characters.Space)
			{
				value = value.SliceUnsafe(1);
			}

			return new KeyValuePair<Unmanaged, Unmanaged>(key, value);
		}

		private static System.ReadOnlySpan<byte> Slice(System.ReadOnlySpan<byte> data, System.ReadOnlySpan<byte> prefix)
		{
			if (data.IsEmpty)
			{
				return default;
			}

			var idx = System.MemoryExtensions.IndexOf(data, prefix);

			return idx == -1 ? default : data.SliceUnsafe(idx + prefix.Length);
		}

		private static void Trim(scoped ref System.ReadOnlySpan<byte> value)
		{
			if (value[0] == Characters.DoubleQuote)
			{
				value = value.SliceUnsafe(1);
			}

			if (value[^1] == Characters.DoubleQuote)
			{
				value = value.SliceUnsafe(0, value.Length - 1);
			}
		}

		public static Multipart FromRequest(in Request request)
		{
			if (!request.TryGetHeader("Content-Type"u8, out var contentType))
			{
				return default;
			}

			return Multipart.FromContentType(contentType, request.Body);
		}

		public static Multipart FromContentType(System.ReadOnlySpan<byte> contentType, System.ReadOnlySpan<byte> data)
		{
			if (!System.MemoryExtensions.StartsWith(contentType, "multipart/form-data"u8))
			{
				return default;
			}

			var boundary = Multipart.Slice(contentType, "boundary="u8);

			Multipart.Trim(ref boundary);

			return new Multipart(boundary, data);
		}

		[MustDisposeResource]
		[StructLayout(LayoutKind.Sequential)]
#if DEBUG
		[System.Diagnostics.DebuggerTypeProxy(typeof(Multipart.Item.DebugView))]
#endif
		public readonly ref struct Item : System.IDisposable
		{
			public readonly System.ReadOnlySpan<byte> Content;

			public readonly RentedArray<KeyValuePair<Unmanaged, Unmanaged>> Headers;

			public System.ReadOnlySpan<byte> Name
			{
				get
				{
					var disposition = this.ContentDisposition;

					if (disposition.IsEmpty)
					{
						return default;
					}

					var name = Multipart.Slice(disposition, "form-data; name="u8);

					// The Content-Disposition header could return extra metadata after the content's name.
					var separatorIdx = System.MemoryExtensions.IndexOf(name, Characters.SemiColon);

					if (separatorIdx != -1)
					{
						name = name.SliceUnsafe(0, separatorIdx);
					}

					Multipart.Trim(ref name);

					return name;
				}
			}

			public System.ReadOnlySpan<byte> ContentDisposition =>
				this.TryGetHeader("Content-Disposition"u8, out var result) ? result : default;

			public System.ReadOnlySpan<byte> ContentType =>
				this.TryGetHeader("Content-Type"u8, out var result) ? result : "text/plain"u8;

			internal Item(System.ReadOnlySpan<byte> content, RentedArray<KeyValuePair<Unmanaged, Unmanaged>> headers)
			{
				this.Content = content;
				this.Headers = headers;
			}

			public bool TryGetHeader(scoped System.ReadOnlySpan<byte> name, out System.ReadOnlySpan<byte> value)
			{
				foreach (var header in this.Headers.Span)
				{
					if (System.MemoryExtensions.SequenceEqual(name, header.Key))
					{
						value = header.Value;
						return true;
					}
				}

				value = default;
				return false;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Dispose() =>
				this.Headers.Dispose();

#if DEBUG
			private sealed class DebugView
			{
				private static readonly System.Text.Encoding enc = System.Text.Encoding.UTF8;

				public readonly string Name;
				public readonly string Content;
				public readonly string ContentDisposition;
				public readonly string ContentType;
				public readonly Dictionary<string, string> Headers;

				public DebugView(Multipart.Item item)
				{
					this.Name = DebugView.enc.GetString(item.Name);
					this.Content = DebugView.enc.GetString(item.Content);
					this.ContentDisposition = DebugView.enc.GetString(item.ContentDisposition);
					this.ContentType = DebugView.enc.GetString(item.ContentType);

					this.Headers = new Dictionary<string, string>(item.Headers.Size, System.StringComparer.Ordinal);

					foreach (var header in item.Headers.Span)
					{
						this.Headers[DebugView.enc.GetString(header.Key)] = DebugView.enc.GetString(header.Value);
					}
				}
			}
#endif
		}
	}
}
