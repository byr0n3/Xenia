namespace Byrone.Xenia
{
	public readonly struct ServerOptions
	{
		public string IpAddress { get; init; }

		public int Port { get; init; }

		public IServerLogger Logger { get; init; }
	}
}
