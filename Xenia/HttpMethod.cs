using JetBrains.Annotations;

namespace Byrone.Xenia
{
	[PublicAPI]
	public enum HttpMethod
	{
		None = 0,
		Get,
		Post,
		Head,
		Put,
		Delete,
		Patch,
		Options,
	}
}
