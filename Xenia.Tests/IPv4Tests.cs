using System.Diagnostics.CodeAnalysis;
using Byrone.Xenia.Utilities;

namespace Byrone.Xenia.Tests
{
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public sealed class IPv4Tests
	{
		[Fact]
		public void AnyAddress()
		{
			Assert.Equal(0, IPv4.Any.A);
			Assert.Equal(0, IPv4.Any.B);
			Assert.Equal(0, IPv4.Any.C);
			Assert.Equal(0, IPv4.Any.D);
		}

		[Fact]
		public void LocalAddress()
		{
			Assert.Equal(127, IPv4.Local.A);
			Assert.Equal(0, IPv4.Local.B);
			Assert.Equal(0, IPv4.Local.C);
			Assert.Equal(1, IPv4.Local.D);
		}

		[Fact]
		public void CanParseAddress()
		{
			var ipv4 = IPv4.Parse("82.141.102.21"u8);

			Assert.Equal(82, ipv4.A);
			Assert.Equal(141, ipv4.B);
			Assert.Equal(102, ipv4.C);
			Assert.Equal(21, ipv4.D);

			Assert.Equal("82.141.102.21", ipv4.ToString());
		}

		[Fact]
		public void CanTryParseAddress()
		{
			Assert.True(IPv4.TryParse("82.141.102.21"u8, out var ipv4));

			Assert.Equal(82, ipv4.A);
			Assert.Equal(141, ipv4.B);
			Assert.Equal(102, ipv4.C);
			Assert.Equal(21, ipv4.D);

			Assert.Equal("82.141.102.21", ipv4.ToString());
		}
	}
}
