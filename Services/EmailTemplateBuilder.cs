using System.Text;

namespace FacultyInformationSystem_FIS_.Services
{
    // Builds a simple branded HTML email — table-based layout with inline
    // styles only, since most email clients (Outlook especially) ignore
    // <style> blocks and modern CSS. Keep this old-school on purpose.
    public static class EmailTemplateBuilder
    {
        public static string Build(string heading, string intro, (string Label, string Value)[] fields)
        {
            var rows = new StringBuilder();
            foreach (var (label, value) in fields)
            {
                rows.Append($@"
                <tr>
                  <td style=""padding:10px 0; border-bottom:1px solid #EAE4D3; font-family:Arial,sans-serif; font-size:13px; font-weight:bold; color:#4A4A46; width:160px; vertical-align:top;"">
                    {System.Net.WebUtility.HtmlEncode(label)}
                  </td>
                  <td style=""padding:10px 0; border-bottom:1px solid #EAE4D3; font-family:Arial,sans-serif; font-size:14px; color:#181818; vertical-align:top; white-space:pre-line;"">
                    {System.Net.WebUtility.HtmlEncode(value)}
                  </td>
                </tr>");
            }

            return $@"
<!DOCTYPE html>
<html>
<body style=""margin:0; padding:0; background-color:#FDFBF6;"">
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#FDFBF6; padding:32px 16px;"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" width=""560"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#FFFFFF; border:1px solid #EAE4D3; border-radius:16px; overflow:hidden;"">

          <tr>
            <td style=""background-color:#C5A63E; padding:22px 28px;"">
              <span style=""font-family:Arial,sans-serif; font-size:13px; font-weight:bold; letter-spacing:0.06em; text-transform:uppercase; color:#181818;"">
                Faculty Information System
              </span>
            </td>
          </tr>

          <tr>
            <td style=""padding:28px 28px 8px;"">
              <h1 style=""margin:0; font-family:Arial,sans-serif; font-size:20px; color:#181818;"">
                {System.Net.WebUtility.HtmlEncode(heading)}
              </h1>
              <p style=""margin:10px 0 0; font-family:Arial,sans-serif; font-size:14px; color:#4A4A46;"">
                {System.Net.WebUtility.HtmlEncode(intro)}
              </p>
            </td>
          </tr>

          <tr>
            <td style=""padding:8px 28px 28px;"">
              <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"">
                {rows}
              </table>
            </td>
          </tr>

          <tr>
            <td style=""background-color:#FDFBF6; padding:16px 28px; border-top:1px solid #EAE4D3;"">
              <p style=""margin:0; font-family:Arial,sans-serif; font-size:12px; color:#8a8a80;"">
                This message was submitted through a form on the Faculty Information System website.
              </p>
            </td>
          </tr>

        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
        }
    }
}
