using JetBrains.Annotations;

namespace Byrone.Xenia.Data
{
	[PublicAPI]
	public enum HttpMethod
	{
		None = 0,
		Get,
		Post,
		Put,
		Delete,
		Patch,

		Head,
		Options,
	}
}
