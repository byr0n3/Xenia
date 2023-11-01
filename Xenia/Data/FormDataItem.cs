using System.Diagnostics;
using System.Runtime.InteropServices;
using Byrone.Xenia.Helpers;
using JetBrains.Annotations;

namespace Byrone.Xenia.Data
{
	[PublicAPI]
	[StructLayout(LayoutKind.Sequential)]
#if DEBUG
	[DebuggerTypeProxy(typeof(FormDataItem.DebugView))]
#endif
	public readonly struct FormDataItem : System.IEquatable<FormDataItem>
	{
		public required SpanPointer<byte> Name { get; init; }

		public required SpanPointer<byte> Content { get; init; }

		public SpanPointer<byte> ContentType { get; init; }

		public SpanPointer<byte> FileName { get; init; }

		public bool Equals(FormDataItem other) =>
			this.Name.Equals(other.Name) &&
			this.ContentType.Equals(other.ContentType) &&
			this.Content.Equals(other.Content) &&
			this.FileName.Equals(other.FileName);

		public override bool Equals(object? @object) =>
			@object is FormDataItem other && this.Equals(other);

		public override int GetHashCode() =>
			System.HashCode.Combine(this.Name, this.ContentType, this.Content, this.FileName);

		public static bool operator ==(in FormDataItem left, in FormDataItem right) =>
			left.Equals(right);

		public static bool operator !=(in FormDataItem left, in FormDataItem right) =>
			!left.Equals(right);

#if DEBUG
		private sealed class DebugView
		{
			private static readonly System.Text.Encoding encoding = System.Text.Encoding.Latin1;

			public required string Name { get; init; }

			public required string Content { get; init; }

			public required string? ContentType { get; init; }

			public required string? FileName { get; init; }

			public DebugView(FormDataItem formData)
			{
				this.Name = DebugView.encoding.GetString(formData.Name);
				this.Content = DebugView.encoding.GetString(formData.Content);

				if (!formData.ContentType.Span.IsEmpty)
				{
					this.ContentType = DebugView.encoding.GetString(formData.ContentType);
				}

				if (!formData.FileName.Span.IsEmpty)
				{
					this.ContentType = DebugView.encoding.GetString(formData.FileName);
				}
			}
		}
#endif
	}
}
