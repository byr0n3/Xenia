namespace Byrone.Xenia.Internal
{
	internal static class Characters
	{
		public const byte PathDelimiter = (byte)'/';
		public const byte QueryParametersDelimiter = (byte)'?';
		public const byte QueryParametersAssignDelimiter = (byte)'=';
		public const byte QueryParametersJoinDelimiter = (byte)'&';

		public const byte RouteParameterStart = (byte)'{';
		public const byte RouteParameterEnd = (byte)'}';

		public const byte Space = (byte)' ';
		public const byte SemiColon = (byte)':';
		public const byte Dot = (byte)'.';
	}
}
