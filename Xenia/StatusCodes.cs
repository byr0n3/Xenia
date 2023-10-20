using System.Diagnostics.CodeAnalysis;
using Byrone.Xenia.Data;
using JetBrains.Annotations;

namespace Byrone.Xenia
{
	[PublicAPI]
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public static class StatusCodes
	{
		/// <summary>HTTP status code 100.</summary>
		public static readonly StatusCode Status100Continue = new(100, "Continue"u8);

		/// <summary>HTTP status code 101.</summary>
		public static readonly StatusCode Status101SwitchingProtocols = new(101, "Switching Protocols"u8);

		/// <summary>HTTP status code 102.</summary>
		public static readonly StatusCode Status102Processing = new(102, "Processing"u8);

		/// <summary>HTTP status code 200.</summary>
		public static readonly StatusCode Status200OK = new(200, "OK"u8);

		/// <summary>HTTP status code 201.</summary>
		public static readonly StatusCode Status201Created = new(201, "Created"u8);

		/// <summary>HTTP status code 202.</summary>
		public static readonly StatusCode Status202Accepted = new(202, "Accepted"u8);

		/// <summary>HTTP status code 203.</summary>
		public static readonly StatusCode Status203NonAuthoritative = new(203, "Non Authoritative"u8);

		/// <summary>HTTP status code 204.</summary>
		public static readonly StatusCode Status204NoContent = new(204, "No Content"u8);

		/// <summary>HTTP status code 205.</summary>
		public static readonly StatusCode Status205ResetContent = new(205, "Reset Content ="u8);

		/// <summary>HTTP status code 206.</summary>
		public static readonly StatusCode Status206PartialContent = new(206, "Partial Content ="u8);

		/// <summary>HTTP status code 207.</summary>
		public static readonly StatusCode Status207MultiStatus = new(207, "Multi Status ="u8);

		/// <summary>HTTP status code 208.</summary>
		public static readonly StatusCode Status208AlreadyReported = new(208, "Already Reported ="u8);

		/// <summary>HTTP status code 226.</summary>
		public static readonly StatusCode Status226IMUsed = new(226, "IM Used"u8);

		/// <summary>HTTP status code 300.</summary>
		public static readonly StatusCode Status300MultipleChoices = new(300, "Multiple Choices"u8);

		/// <summary>HTTP status code 301.</summary>
		public static readonly StatusCode Status301MovedPermanently = new(301, "Moved Permanently"u8);

		/// <summary>HTTP status code 302.</summary>
		public static readonly StatusCode Status302Found = new(302, "Found"u8);

		/// <summary>HTTP status code 303.</summary>
		public static readonly StatusCode Status303SeeOther = new(303, "See Other"u8);

		/// <summary>HTTP status code 304.</summary>
		public static readonly StatusCode Status304NotModified = new(304, "Not Modified"u8);

		/// <summary>HTTP status code 305.</summary>
		public static readonly StatusCode Status305UseProxy = new(305, "Use Proxy"u8);

		/// <summary>HTTP status code 306.</summary>
		public static readonly StatusCode Status306SwitchProxy = new(306, "Switch Proxy"u8);

		/// <summary>HTTP status code 307.</summary>
		public static readonly StatusCode Status307TemporaryRedirect = new(307, "Temporary Redirect"u8);

		/// <summary>HTTP status code 308.</summary>
		public static readonly StatusCode Status308PermanentRedirect = new(308, "Permanent Redirect"u8);

		/// <summary>HTTP status code 400.</summary>
		public static readonly StatusCode Status400BadRequest = new(400, "Bad Request"u8);

		/// <summary>HTTP status code 401.</summary>
		public static readonly StatusCode Status401Unauthorized = new(401, "Unauthorized"u8);

		/// <summary>HTTP status code 402.</summary>
		public static readonly StatusCode Status402PaymentRequired = new(402, "Payment Required"u8);

		/// <summary>HTTP status code 403.</summary>
		public static readonly StatusCode Status403Forbidden = new(403, "Forbidden"u8);

		/// <summary>HTTP status code 404.</summary>
		public static readonly StatusCode Status404NotFound = new(404, "Not Found"u8);

		/// <summary>HTTP status code 405.</summary>
		public static readonly StatusCode Status405MethodNotAllowed = new(405, "Method Not Allowed"u8);

		/// <summary>HTTP status code 406.</summary>
		public static readonly StatusCode Status406NotAcceptable = new(406, "Not Acceptable"u8);

		/// <summary>HTTP status code 407.</summary>
		public static readonly StatusCode Status407ProxyAuthenticationRequired = new(407, "Proxy Authentication Required"u8);

		/// <summary>HTTP status code 408.</summary>
		public static readonly StatusCode Status408RequestTimeout = new(408, "Request Timeout"u8);

		/// <summary>HTTP status code 409.</summary>
		public static readonly StatusCode Status409Conflict = new(409, "Conflict"u8);

		/// <summary>HTTP status code 410.</summary>
		public static readonly StatusCode Status410Gone = new(410, "Gone"u8);

		/// <summary>HTTP status code 411.</summary>
		public static readonly StatusCode Status411LengthRequired = new(411, "Length Required"u8);

		/// <summary>HTTP status code 412.</summary>
		public static readonly StatusCode Status412PreconditionFailed = new(412, "Precondition Failed"u8);

		/// <summary>HTTP status code 413.</summary>
		public static readonly StatusCode Status413RequestEntityTooLarge = new(413, "Request Entity Too Large"u8);

		/// <summary>HTTP status code 413.</summary>
		public static readonly StatusCode Status413PayloadTooLarge = new(413, "Payload Too Large"u8);

		/// <summary>HTTP status code 414.</summary>
		public static readonly StatusCode Status414RequestUriTooLong = new(414, "Request Uri Too"u8);

		/// <summary>HTTP status code 414.</summary>
		public static readonly StatusCode Status414UriTooLong = new(414, "Uri Too Long"u8);

		/// <summary>HTTP status code 415.</summary>
		public static readonly StatusCode Status415UnsupportedMediaType = new(415, "Unsupported Media Type"u8);

		/// <summary>HTTP status code 416.</summary>
		public static readonly StatusCode Status416RequestedRangeNotSatisfiable = new(416, "Requested Range Not"u8);

		/// <summary>HTTP status code 416.</summary>
		public static readonly StatusCode Status416RangeNotSatisfiable = new(416, "Range Not Satisfiable"u8);

		/// <summary>HTTP status code 417.</summary>
		public static readonly StatusCode Status417ExpectationFailed = new(417, "Expectation Failed"u8);

		/// <summary>HTTP status code 418.</summary>
		public static readonly StatusCode Status418ImATeapot = new(418, "Im A Teapot"u8);

		/// <summary>HTTP status code 419.</summary>
		public static readonly StatusCode Status419AuthenticationTimeout = new(419, "Authentication Timeout"u8);

		/// <summary>HTTP status code 421.</summary>
		public static readonly StatusCode Status421MisdirectedRequest = new(421, "Misdirected Request"u8);

		/// <summary>HTTP status code 422.</summary>
		public static readonly StatusCode Status422UnprocessableEntity = new(422, "Unprocessable Entity"u8);

		/// <summary>HTTP status code 423.</summary>
		public static readonly StatusCode Status423Locked = new(423, "Locked"u8);

		/// <summary>HTTP status code 424.</summary>
		public static readonly StatusCode Status424FailedDependency = new(424, "Failed Dependency"u8);

		/// <summary>HTTP status code 426.</summary>
		public static readonly StatusCode Status426UpgradeRequired = new(426, "Upgrade Required"u8);

		/// <summary>HTTP status code 428.</summary>
		public static readonly StatusCode Status428PreconditionRequired = new(428, "Precondition Required"u8);

		/// <summary>HTTP status code 429.</summary>
		public static readonly StatusCode Status429TooManyRequests = new(429, "Too Many Requests"u8);

		/// <summary>HTTP status code 431.</summary>
		public static readonly StatusCode Status431RequestHeaderFieldsTooLarge = new(431, "Request Header Fields"u8);

		/// <summary>HTTP status code 451.</summary>
		public static readonly StatusCode Status451UnavailableForLegalReasons = new(451, "Unavailable For Legal"u8);

		/// <summary>
		/// HTTP status code 499. This is an unofficial status code originally defined by Nginx and is commonly used
		/// in logs when the client has disconnected.
		/// </summary>
		public static readonly StatusCode Status499ClientClosedRequest = new(499, "Client Closed Request"u8);

		/// <summary>HTTP status code 500.</summary>
		public static readonly StatusCode Status500InternalServerError = new(500, "Internal Server Error"u8);

		/// <summary>HTTP status code 501.</summary>
		public static readonly StatusCode Status501NotImplemented = new(501, "Not Implemented"u8);

		/// <summary>HTTP status code 502.</summary>
		public static readonly StatusCode Status502BadGateway = new(502, "Bad Gateway"u8);

		/// <summary>HTTP status code 503.</summary>
		public static readonly StatusCode Status503ServiceUnavailable = new(503, "Service Unavailable"u8);

		/// <summary>HTTP status code 504.</summary>
		public static readonly StatusCode Status504GatewayTimeout = new(504, "Gateway Timeout"u8);

		/// <summary>HTTP status code 505.</summary>
		public static readonly StatusCode Status505HTTPVersionNotSupported = new(505, "HTTP Version Not Supported"u8);

		/// <summary>HTTP status code 506.</summary>
		public static readonly StatusCode Status506VariantAlsoNegotiates = new(506, "Variant Also Negotiates"u8);

		/// <summary>HTTP status code 507.</summary>
		public static readonly StatusCode Status507InsufficientStorage = new(507, "Insufficient Storage"u8);

		/// <summary>HTTP status code 508.</summary>
		public static readonly StatusCode Status508LoopDetected = new(508, "Loop Detected"u8);

		/// <summary>HTTP status code 510.</summary>
		public static readonly StatusCode Status510NotExtended = new(510, "Not Extended"u8);

		/// <summary>HTTP status code 511.</summary>
		public static readonly StatusCode Status511NetworkAuthenticationRequired = new(511, "Network Authentication Required"u8);
	}
}
