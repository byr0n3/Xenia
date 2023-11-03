//------------------------------------------------------------------------------
// <copyright file="MimeMapping.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*++
		MimeMapping : This module maps document extensions to Content Mime Type.
		Copyright (c) 2011 Microsoft Corporation
--*/

// Copied over from official source so we don't have to install a dependency for just this
// Also modified to fit our needs

using Byrone.Xenia.Helpers;
using System.Collections.Generic;
using System.IO;

namespace Byrone.Xenia.Internal
{
	internal static class MimeMapping
	{
		private static readonly MimeMappingDictionaryBase mappingDictionary = new MimeMappingDictionaryClassic();

		public static System.ReadOnlySpan<byte> GetMimeMapping(string fileName) =>
			MimeMapping.mappingDictionary.GetMimeMapping(fileName);

		private abstract class MimeMappingDictionaryBase
		{
			private readonly Dictionary<string, SpanPointer<byte>> mappings =
				new(System.StringComparer.OrdinalIgnoreCase);

			private static readonly char[] pathSeparatorChars =
			{
				Path.DirectorySeparatorChar,
				Path.AltDirectorySeparatorChar,
				Path.VolumeSeparatorChar,
			}; // from Path.GetFileName()

			private bool isInitialized;

			protected void AddMapping(string fileExtension, System.ReadOnlySpan<byte> mimeType) =>
				this.mappings.Add(fileExtension, mimeType);

			private void AddWildcardIfNotPresent()
			{
				if (!this.mappings.ContainsKey(".*"))
				{
					this.AddMapping(".*", "application/octet-stream"u8);
				}
			}

			private void EnsureMapping()
			{
				// Ensure initialized only once
				if (this.isInitialized)
				{
					return;
				}

				lock (this)
				{
					if (this.isInitialized)
					{
						return;
					}

					this.PopulateMappings();
					this.AddWildcardIfNotPresent();
					this.isInitialized = true;
				}
			}

			// The derived class will override this method and call AddMapping for each mapping it knows about
			protected abstract void PopulateMappings();

			// This method is similar to Path.GetFileName(), but it doesn't fail on invalid path characters
			private static string GetFileName(string path)
			{
				var pathSeparatorIndex = path.LastIndexOfAny(MimeMappingDictionaryBase.pathSeparatorChars);
				return (pathSeparatorIndex >= 0) ? path[pathSeparatorIndex..] : path;
			}

			public System.ReadOnlySpan<byte> GetMimeMapping(string fileName)
			{
				this.EnsureMapping();
				fileName = MimeMappingDictionaryBase.GetFileName(fileName); // strip off path separators

				// some MIME types have complex extensions (like ".exe.config"), so we need to work left-to-right
				for (var i = 0; i < fileName.Length; i++)
				{
					if (fileName[i] != '.')
					{
						continue;
					}

					if (this.mappings.TryGetValue(fileName[i..], out var mimeType))
					{
						return mimeType;
					}
				}

				// If we reached this point, either we couldn't find an extension, or the extension we found
				// wasn't recognized. In either case, the ".*" mapping is guaranteed to exist as a fallback.
				return this.mappings[".*"];
			}
		}

		// This can provide fallback mappings if we don't have an actual applicationHost.config from which to read
		private sealed class MimeMappingDictionaryClassic : MimeMappingDictionaryBase
		{
			protected override void PopulateMappings()
			{
				// This list was copied from the IIS7 configuration file located at:
				// %windir%\system32\inetsrv\config\applicationHost.config

				this.AddMapping(".323", "text/h323"u8);
				this.AddMapping(".aaf", "application/octet-stream"u8);
				this.AddMapping(".aca", "application/octet-stream"u8);
				this.AddMapping(".accdb", "application/msaccess"u8);
				this.AddMapping(".accde", "application/msaccess"u8);
				this.AddMapping(".accdt", "application/msaccess"u8);
				this.AddMapping(".acx", "application/internet-property-stream"u8);
				this.AddMapping(".afm", "application/octet-stream"u8);
				this.AddMapping(".ai", "application/postscript"u8);
				this.AddMapping(".aif", "audio/x-aiff"u8);
				this.AddMapping(".aifc", "audio/aiff"u8);
				this.AddMapping(".aiff", "audio/aiff"u8);
				this.AddMapping(".application", "application/x-ms-application"u8);
				this.AddMapping(".art", "image/x-jg"u8);
				this.AddMapping(".asd", "application/octet-stream"u8);
				this.AddMapping(".asf", "video/x-ms-asf"u8);
				this.AddMapping(".asi", "application/octet-stream"u8);
				this.AddMapping(".asm", "text/plain"u8);
				this.AddMapping(".asr", "video/x-ms-asf"u8);
				this.AddMapping(".asx", "video/x-ms-asf"u8);
				this.AddMapping(".atom", "application/atom+xml"u8);
				this.AddMapping(".au", "audio/basic"u8);
				this.AddMapping(".avi", "video/x-msvideo"u8);
				this.AddMapping(".axs", "application/olescript"u8);
				this.AddMapping(".bas", "text/plain"u8);
				this.AddMapping(".bcpio", "application/x-bcpio"u8);
				this.AddMapping(".bin", "application/octet-stream"u8);
				this.AddMapping(".bmp", "image/bmp"u8);
				this.AddMapping(".c", "text/plain"u8);
				this.AddMapping(".cab", "application/octet-stream"u8);
				this.AddMapping(".calx", "application/vnd.ms-office.calx"u8);
				this.AddMapping(".cat", "application/vnd.ms-pki.seccat"u8);
				this.AddMapping(".cdf", "application/x-cdf"u8);
				this.AddMapping(".chm", "application/octet-stream"u8);
				this.AddMapping(".class", "application/x-java-applet"u8);
				this.AddMapping(".clp", "application/x-msclip"u8);
				this.AddMapping(".cmx", "image/x-cmx"u8);
				this.AddMapping(".cnf", "text/plain"u8);
				this.AddMapping(".cod", "image/cis-cod"u8);
				this.AddMapping(".cpio", "application/x-cpio"u8);
				this.AddMapping(".cpp", "text/plain"u8);
				this.AddMapping(".crd", "application/x-mscardfile"u8);
				this.AddMapping(".crl", "application/pkix-crl"u8);
				this.AddMapping(".crt", "application/x-x509-ca-cert"u8);
				this.AddMapping(".csh", "application/x-csh"u8);
				this.AddMapping(".css", "text/css"u8);
				this.AddMapping(".csv", "application/octet-stream"u8);
				this.AddMapping(".cur", "application/octet-stream"u8);
				this.AddMapping(".dcr", "application/x-director"u8);
				this.AddMapping(".deploy", "application/octet-stream"u8);
				this.AddMapping(".der", "application/x-x509-ca-cert"u8);
				this.AddMapping(".dib", "image/bmp"u8);
				this.AddMapping(".dir", "application/x-director"u8);
				this.AddMapping(".disco", "text/xml"u8);
				this.AddMapping(".dll", "application/x-msdownload"u8);
				this.AddMapping(".dll.config", "text/xml"u8);
				this.AddMapping(".dlm", "text/dlm"u8);
				this.AddMapping(".doc", "application/msword"u8);
				this.AddMapping(".docm", "application/vnd.ms-word.document.macroEnabled.12"u8);
				this.AddMapping(".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"u8);
				this.AddMapping(".dot", "application/msword"u8);
				this.AddMapping(".dotm", "application/vnd.ms-word.template.macroEnabled.12"u8);
				this.AddMapping(".dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template"u8);
				this.AddMapping(".dsp", "application/octet-stream"u8);
				this.AddMapping(".dtd", "text/xml"u8);
				this.AddMapping(".dvi", "application/x-dvi"u8);
				this.AddMapping(".dwf", "drawing/x-dwf"u8);
				this.AddMapping(".dwp", "application/octet-stream"u8);
				this.AddMapping(".dxr", "application/x-director"u8);
				this.AddMapping(".eml", "message/rfc822"u8);
				this.AddMapping(".emz", "application/octet-stream"u8);
				this.AddMapping(".eot", "application/octet-stream"u8);
				this.AddMapping(".eps", "application/postscript"u8);
				this.AddMapping(".etx", "text/x-setext"u8);
				this.AddMapping(".evy", "application/envoy"u8);
				this.AddMapping(".exe", "application/octet-stream"u8);
				this.AddMapping(".exe.config", "text/xml"u8);
				this.AddMapping(".fdf", "application/vnd.fdf"u8);
				this.AddMapping(".fif", "application/fractals"u8);
				this.AddMapping(".fla", "application/octet-stream"u8);
				this.AddMapping(".flr", "x-world/x-vrml"u8);
				this.AddMapping(".flv", "video/x-flv"u8);
				this.AddMapping(".gif", "image/gif"u8);
				this.AddMapping(".gtar", "application/x-gtar"u8);
				this.AddMapping(".gz", "application/x-gzip"u8);
				this.AddMapping(".h", "text/plain"u8);
				this.AddMapping(".hdf", "application/x-hdf"u8);
				this.AddMapping(".hdml", "text/x-hdml"u8);
				this.AddMapping(".hhc", "application/x-oleobject"u8);
				this.AddMapping(".hhk", "application/octet-stream"u8);
				this.AddMapping(".hhp", "application/octet-stream"u8);
				this.AddMapping(".hlp", "application/winhlp"u8);
				this.AddMapping(".hqx", "application/mac-binhex40"u8);
				this.AddMapping(".hta", "application/hta"u8);
				this.AddMapping(".htc", "text/x-component"u8);
				this.AddMapping(".htm", "text/html"u8);
				this.AddMapping(".html", "text/html"u8);
				this.AddMapping(".htt", "text/webviewhtml"u8);
				this.AddMapping(".hxt", "text/html"u8);
				this.AddMapping(".ico", "image/x-icon"u8);
				this.AddMapping(".ics", "application/octet-stream"u8);
				this.AddMapping(".ief", "image/ief"u8);
				this.AddMapping(".iii", "application/x-iphone"u8);
				this.AddMapping(".inf", "application/octet-stream"u8);
				this.AddMapping(".ins", "application/x-internet-signup"u8);
				this.AddMapping(".isp", "application/x-internet-signup"u8);
				this.AddMapping(".IVF", "video/x-ivf"u8);
				this.AddMapping(".jar", "application/java-archive"u8);
				this.AddMapping(".java", "application/octet-stream"u8);
				this.AddMapping(".jck", "application/liquidmotion"u8);
				this.AddMapping(".jcz", "application/liquidmotion"u8);
				this.AddMapping(".jfif", "image/pjpeg"u8);
				this.AddMapping(".jpb", "application/octet-stream"u8);
				this.AddMapping(".jpe", "image/jpeg"u8);
				this.AddMapping(".jpeg", "image/jpeg"u8);
				this.AddMapping(".jpg", "image/jpeg"u8);
				this.AddMapping(".js", "application/javascript"u8);
				this.AddMapping(".jsx", "text/jscript"u8);
				this.AddMapping(".latex", "application/x-latex"u8);
				this.AddMapping(".lit", "application/x-ms-reader"u8);
				this.AddMapping(".lpk", "application/octet-stream"u8);
				this.AddMapping(".lsf", "video/x-la-asf"u8);
				this.AddMapping(".lsx", "video/x-la-asf"u8);
				this.AddMapping(".lzh", "application/octet-stream"u8);
				this.AddMapping(".m13", "application/x-msmediaview"u8);
				this.AddMapping(".m14", "application/x-msmediaview"u8);
				this.AddMapping(".m1v", "video/mpeg"u8);
				this.AddMapping(".m3u", "audio/x-mpegurl"u8);
				this.AddMapping(".man", "application/x-troff-man"u8);
				this.AddMapping(".manifest", "application/x-ms-manifest"u8);
				this.AddMapping(".map", "text/plain"u8);
				this.AddMapping(".mdb", "application/x-msaccess"u8);
				this.AddMapping(".mdp", "application/octet-stream"u8);
				this.AddMapping(".me", "application/x-troff-me"u8);
				this.AddMapping(".mht", "message/rfc822"u8);
				this.AddMapping(".mhtml", "message/rfc822"u8);
				this.AddMapping(".mid", "audio/mid"u8);
				this.AddMapping(".midi", "audio/mid"u8);
				this.AddMapping(".mix", "application/octet-stream"u8);
				this.AddMapping(".mmf", "application/x-smaf"u8);
				this.AddMapping(".mno", "text/xml"u8);
				this.AddMapping(".mny", "application/x-msmoney"u8);
				this.AddMapping(".mov", "video/quicktime"u8);
				this.AddMapping(".movie", "video/x-sgi-movie"u8);
				this.AddMapping(".mp2", "video/mpeg"u8);
				this.AddMapping(".mp3", "audio/mpeg"u8);
				this.AddMapping(".mpa", "video/mpeg"u8);
				this.AddMapping(".mpe", "video/mpeg"u8);
				this.AddMapping(".mpeg", "video/mpeg"u8);
				this.AddMapping(".mpg", "video/mpeg"u8);
				this.AddMapping(".mpp", "application/vnd.ms-project"u8);
				this.AddMapping(".mpv2", "video/mpeg"u8);
				this.AddMapping(".ms", "application/x-troff-ms"u8);
				this.AddMapping(".msi", "application/octet-stream"u8);
				this.AddMapping(".mso", "application/octet-stream"u8);
				this.AddMapping(".mvb", "application/x-msmediaview"u8);
				this.AddMapping(".mvc", "application/x-miva-compiled"u8);
				this.AddMapping(".nc", "application/x-netcdf"u8);
				this.AddMapping(".nsc", "video/x-ms-asf"u8);
				this.AddMapping(".nws", "message/rfc822"u8);
				this.AddMapping(".ocx", "application/octet-stream"u8);
				this.AddMapping(".oda", "application/oda"u8);
				this.AddMapping(".odc", "text/x-ms-odc"u8);
				this.AddMapping(".ods", "application/oleobject"u8);
				this.AddMapping(".one", "application/onenote"u8);
				this.AddMapping(".onea", "application/onenote"u8);
				this.AddMapping(".onetoc", "application/onenote"u8);
				this.AddMapping(".onetoc2", "application/onenote"u8);
				this.AddMapping(".onetmp", "application/onenote"u8);
				this.AddMapping(".onepkg", "application/onenote"u8);
				this.AddMapping(".osdx", "application/opensearchdescription+xml"u8);
				this.AddMapping(".p10", "application/pkcs10"u8);
				this.AddMapping(".p12", "application/x-pkcs12"u8);
				this.AddMapping(".p7b", "application/x-pkcs7-certificates"u8);
				this.AddMapping(".p7c", "application/pkcs7-mime"u8);
				this.AddMapping(".p7m", "application/pkcs7-mime"u8);
				this.AddMapping(".p7r", "application/x-pkcs7-certreqresp"u8);
				this.AddMapping(".p7s", "application/pkcs7-signature"u8);
				this.AddMapping(".pbm", "image/x-portable-bitmap"u8);
				this.AddMapping(".pcx", "application/octet-stream"u8);
				this.AddMapping(".pcz", "application/octet-stream"u8);
				this.AddMapping(".pdf", "application/pdf"u8);
				this.AddMapping(".pfb", "application/octet-stream"u8);
				this.AddMapping(".pfm", "application/octet-stream"u8);
				this.AddMapping(".pfx", "application/x-pkcs12"u8);
				this.AddMapping(".pgm", "image/x-portable-graymap"u8);
				this.AddMapping(".pko", "application/vnd.ms-pki.pko"u8);
				this.AddMapping(".pma", "application/x-perfmon"u8);
				this.AddMapping(".pmc", "application/x-perfmon"u8);
				this.AddMapping(".pml", "application/x-perfmon"u8);
				this.AddMapping(".pmr", "application/x-perfmon"u8);
				this.AddMapping(".pmw", "application/x-perfmon"u8);
				this.AddMapping(".png", "image/png"u8);
				this.AddMapping(".pnm", "image/x-portable-anymap"u8);
				this.AddMapping(".pnz", "image/png"u8);
				this.AddMapping(".pot", "application/vnd.ms-powerpoint"u8);
				this.AddMapping(".potm", "application/vnd.ms-powerpoint.template.macroEnabled.12"u8);
				this.AddMapping(".potx", "application/vnd.openxmlformats-officedocument.presentationml.template"u8);
				this.AddMapping(".ppam", "application/vnd.ms-powerpoint.addin.macroEnabled.12"u8);
				this.AddMapping(".ppm", "image/x-portable-pixmap"u8);
				this.AddMapping(".pps", "application/vnd.ms-powerpoint"u8);
				this.AddMapping(".ppsm", "application/vnd.ms-powerpoint.slideshow.macroEnabled.12"u8);
				this.AddMapping(".ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow"u8);
				this.AddMapping(".ppt", "application/vnd.ms-powerpoint"u8);
				this.AddMapping(".pptm", "application/vnd.ms-powerpoint.presentation.macroEnabled.12"u8);
				this.AddMapping(".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation"u8);
				this.AddMapping(".prf", "application/pics-rules"u8);
				this.AddMapping(".prm", "application/octet-stream"u8);
				this.AddMapping(".prx", "application/octet-stream"u8);
				this.AddMapping(".ps", "application/postscript"u8);
				this.AddMapping(".psd", "application/octet-stream"u8);
				this.AddMapping(".psm", "application/octet-stream"u8);
				this.AddMapping(".psp", "application/octet-stream"u8);
				this.AddMapping(".pub", "application/x-mspublisher"u8);
				this.AddMapping(".qt", "video/quicktime"u8);
				this.AddMapping(".qtl", "application/x-quicktimeplayer"u8);
				this.AddMapping(".qxd", "application/octet-stream"u8);
				this.AddMapping(".ra", "audio/x-pn-realaudio"u8);
				this.AddMapping(".ram", "audio/x-pn-realaudio"u8);
				this.AddMapping(".rar", "application/octet-stream"u8);
				this.AddMapping(".ras", "image/x-cmu-raster"u8);
				this.AddMapping(".rf", "image/vnd.rn-realflash"u8);
				this.AddMapping(".rgb", "image/x-rgb"u8);
				this.AddMapping(".rm", "application/vnd.rn-realmedia"u8);
				this.AddMapping(".rmi", "audio/mid"u8);
				this.AddMapping(".roff", "application/x-troff"u8);
				this.AddMapping(".rpm", "audio/x-pn-realaudio-plugin"u8);
				this.AddMapping(".rtf", "application/rtf"u8);
				this.AddMapping(".rtx", "text/richtext"u8);
				this.AddMapping(".scd", "application/x-msschedule"u8);
				this.AddMapping(".sct", "text/scriptlet"u8);
				this.AddMapping(".sea", "application/octet-stream"u8);
				this.AddMapping(".setpay", "application/set-payment-initiation"u8);
				this.AddMapping(".setreg", "application/set-registration-initiation"u8);
				this.AddMapping(".sgml", "text/sgml"u8);
				this.AddMapping(".sh", "application/x-sh"u8);
				this.AddMapping(".shar", "application/x-shar"u8);
				this.AddMapping(".sit", "application/x-stuffit"u8);
				this.AddMapping(".sldm", "application/vnd.ms-powerpoint.slide.macroEnabled.12"u8);
				this.AddMapping(".sldx", "application/vnd.openxmlformats-officedocument.presentationml.slide"u8);
				this.AddMapping(".smd", "audio/x-smd"u8);
				this.AddMapping(".smi", "application/octet-stream"u8);
				this.AddMapping(".smx", "audio/x-smd"u8);
				this.AddMapping(".smz", "audio/x-smd"u8);
				this.AddMapping(".snd", "audio/basic"u8);
				this.AddMapping(".snp", "application/octet-stream"u8);
				this.AddMapping(".spc", "application/x-pkcs7-certificates"u8);
				this.AddMapping(".spl", "application/futuresplash"u8);
				this.AddMapping(".src", "application/x-wais-source"u8);
				this.AddMapping(".ssm", "application/streamingmedia"u8);
				this.AddMapping(".sst", "application/vnd.ms-pki.certstore"u8);
				this.AddMapping(".stl", "application/vnd.ms-pki.stl"u8);
				this.AddMapping(".sv4cpio", "application/x-sv4cpio"u8);
				this.AddMapping(".sv4crc", "application/x-sv4crc"u8);
				this.AddMapping(".swf", "application/x-shockwave-flash"u8);
				this.AddMapping(".t", "application/x-troff"u8);
				this.AddMapping(".tar", "application/x-tar"u8);
				this.AddMapping(".tcl", "application/x-tcl"u8);
				this.AddMapping(".tex", "application/x-tex"u8);
				this.AddMapping(".texi", "application/x-texinfo"u8);
				this.AddMapping(".texinfo", "application/x-texinfo"u8);
				this.AddMapping(".tgz", "application/x-compressed"u8);
				this.AddMapping(".thmx", "application/vnd.ms-officetheme"u8);
				this.AddMapping(".thn", "application/octet-stream"u8);
				this.AddMapping(".tif", "image/tiff"u8);
				this.AddMapping(".tiff", "image/tiff"u8);
				this.AddMapping(".toc", "application/octet-stream"u8);
				this.AddMapping(".tr", "application/x-troff"u8);
				this.AddMapping(".trm", "application/x-msterminal"u8);
				this.AddMapping(".tsv", "text/tab-separated-values"u8);
				this.AddMapping(".ttf", "application/octet-stream"u8);
				this.AddMapping(".txt", "text/plain"u8);
				this.AddMapping(".u32", "application/octet-stream"u8);
				this.AddMapping(".uls", "text/iuls"u8);
				this.AddMapping(".ustar", "application/x-ustar"u8);
				this.AddMapping(".vbs", "text/vbscript"u8);
				this.AddMapping(".vcf", "text/x-vcard"u8);
				this.AddMapping(".vcs", "text/plain"u8);
				this.AddMapping(".vdx", "application/vnd.ms-visio.viewer"u8);
				this.AddMapping(".vml", "text/xml"u8);
				this.AddMapping(".vsd", "application/vnd.visio"u8);
				this.AddMapping(".vss", "application/vnd.visio"u8);
				this.AddMapping(".vst", "application/vnd.visio"u8);
				this.AddMapping(".vsto", "application/x-ms-vsto"u8);
				this.AddMapping(".vsw", "application/vnd.visio"u8);
				this.AddMapping(".vsx", "application/vnd.visio"u8);
				this.AddMapping(".vtx", "application/vnd.visio"u8);
				this.AddMapping(".wav", "audio/wav"u8);
				this.AddMapping(".wax", "audio/x-ms-wax"u8);
				this.AddMapping(".wbmp", "image/vnd.wap.wbmp"u8);
				this.AddMapping(".wcm", "application/vnd.ms-works"u8);
				this.AddMapping(".wdb", "application/vnd.ms-works"u8);
				this.AddMapping(".wks", "application/vnd.ms-works"u8);
				this.AddMapping(".wm", "video/x-ms-wm"u8);
				this.AddMapping(".wma", "audio/x-ms-wma"u8);
				this.AddMapping(".wmd", "application/x-ms-wmd"u8);
				this.AddMapping(".wmf", "application/x-msmetafile"u8);
				this.AddMapping(".wml", "text/vnd.wap.wml"u8);
				this.AddMapping(".wmlc", "application/vnd.wap.wmlc"u8);
				this.AddMapping(".wmls", "text/vnd.wap.wmlscript"u8);
				this.AddMapping(".wmlsc", "application/vnd.wap.wmlscriptc"u8);
				this.AddMapping(".wmp", "video/x-ms-wmp"u8);
				this.AddMapping(".wmv", "video/x-ms-wmv"u8);
				this.AddMapping(".wmx", "video/x-ms-wmx"u8);
				this.AddMapping(".wmz", "application/x-ms-wmz"u8);
				this.AddMapping(".wps", "application/vnd.ms-works"u8);
				this.AddMapping(".wri", "application/x-mswrite"u8);
				this.AddMapping(".wrl", "x-world/x-vrml"u8);
				this.AddMapping(".wrz", "x-world/x-vrml"u8);
				this.AddMapping(".wsdl", "text/xml"u8);
				this.AddMapping(".wvx", "video/x-ms-wvx"u8);
				this.AddMapping(".x", "application/directx"u8);
				this.AddMapping(".xaf", "x-world/x-vrml"u8);
				this.AddMapping(".xaml", "application/xaml+xml"u8);
				this.AddMapping(".xap", "application/x-silverlight-app"u8);
				this.AddMapping(".xbap", "application/x-ms-xbap"u8);
				this.AddMapping(".xbm", "image/x-xbitmap"u8);
				this.AddMapping(".xdr", "text/plain"u8);
				this.AddMapping(".xla", "application/vnd.ms-excel"u8);
				this.AddMapping(".xlam", "application/vnd.ms-excel.addin.macroEnabled.12"u8);
				this.AddMapping(".xlc", "application/vnd.ms-excel"u8);
				this.AddMapping(".xlm", "application/vnd.ms-excel"u8);
				this.AddMapping(".xls", "application/vnd.ms-excel"u8);
				this.AddMapping(".xlsb", "application/vnd.ms-excel.sheet.binary.macroEnabled.12"u8);
				this.AddMapping(".xlsm", "application/vnd.ms-excel.sheet.macroEnabled.12"u8);
				this.AddMapping(".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"u8);
				this.AddMapping(".xlt", "application/vnd.ms-excel"u8);
				this.AddMapping(".xltm", "application/vnd.ms-excel.template.macroEnabled.12"u8);
				this.AddMapping(".xltx", "application/vnd.openxmlformats-officedocument.spreadsheetml.template"u8);
				this.AddMapping(".xlw", "application/vnd.ms-excel"u8);
				this.AddMapping(".xml", "text/xml"u8);
				this.AddMapping(".xof", "x-world/x-vrml"u8);
				this.AddMapping(".xpm", "image/x-xpixmap"u8);
				this.AddMapping(".xps", "application/vnd.ms-xpsdocument"u8);
				this.AddMapping(".xsd", "text/xml"u8);
				this.AddMapping(".xsf", "text/xml"u8);
				this.AddMapping(".xsl", "text/xml"u8);
				this.AddMapping(".xslt", "text/xml"u8);
				this.AddMapping(".xsn", "application/octet-stream"u8);
				this.AddMapping(".xtp", "application/octet-stream"u8);
				this.AddMapping(".xwd", "image/x-xwindowdump"u8);
				this.AddMapping(".z", "application/x-compress"u8);
				this.AddMapping(".zip", "application/x-zip-compressed"u8);
			}
		}
	}
}
