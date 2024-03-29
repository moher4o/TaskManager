﻿using System.Collections.Generic;
using System.IO;

namespace TaskManager.Common
{
    public static class DataConstants
    {

        public const int TotalHoursPerDay = 12;

        public const string SuperAdmin = "SuperAdmin";
        public const string DirectorateAdmin = "DirectorateAdmin";
        public const string DepartmentAdmin = "DepartmentAdmin";
        public const string SectorAdmin = "SectorAdmin";
        public const string Employee = "Employee";
        public const int RolesCount = 5;  // броя на ролите по-горе   !!!!!

        public const string TaskStatusNew = "Нова";
        public const string TaskStatusInProgres = "В изпълнение";
        public const string TaskStatusClosed = "Приключена";
        public const int TasksStatusCount = 3; // броя на статусите по-горе  !!!

        public const string TaskTypeSpecificWork = "Специфична дейност";
        public const string TaskTypeProcurement = "Обществени поръчки и проекти";
        public const string TaskTypeLearning =    "Обучения и Презентации";
        public const string TaskTypeAdminActivity = "Административна дейност";
        public const string TaskTypeMeetings =    "Работни групи и Комисии";
        public const string TaskTypeOther =       "Други дейности";
        public const string TaskTypeGlobal =      "Глобална";
        public const string TaskTypeSystem =      "Системен тип";
        public const string TaskTypeSpecialTeam = "Специализиран екип по проект";
        public const int TasksTypesCount = 9; // броя на типовете по-горе  !!!

        public const string TaskPriorityLow = "Нисък";
        public const string TaskPriorityNormal = "Нормален";
        public const string TaskPriorityHi = "Висок";
        public const int TasksPriorityCount = 3; // броя на типовете по-горе  !!!

        public const string ChooseValue = "Моля изберете...";


        //Developer Data
        public const string DeveloperUsername = "dc1\\avukov";
        public const string DeveloperEmail = "avukov@e-gov.bg";
        public const string DeveloperFirstName = "Ангел";
        public const string DeveloperLastName = "Вуков";
        //public const string DeveloperJobTitle = "Employee"; //for first use
        public const string SystemUsername = "taskmanagersystemUser";
        public const string SystemEmail = "notusedmail@e-gov.bg";
        public const string SystemName = "Taskmanager";

        public const string FirmName = "Мениджър задачи";
        //public const string ClassFilesSubDirectory = "TasksFiles";


        //Email настройки        
        //public const string FromEmailString = "noreplay-taskmanager@e-gov.bg";

        public const string NotificationTemplate = @"
                            <div class=""col-md-offset-1"">
                            <p>Здравейте,  </p>
                            <p>Добавен е коментар по задача: <p> {0}</p></p>
                            <p>За вход в системата, ползвайте домейнския си акаунт и парола</p>
                            </div>

                            <div>
                            <table>
                                <tbody>
                                    <tr>
                                        <td><img src=""{2}"" alt=""Useful alt text"" width=""70"" height=""100"" border=""0"" style=""border:0; outline:none; text-decoration:none; display:block;"">
                                        </td>
                                        <td style = ""padding-left:10px;"" >
                                            <div>
                                                <div class=""logo-text""><a href = ""{1}"">Мениджър задачи</a></div>
                                                <div class=""logo-text-secondary"">Изпълнителна агенция ""Инфраструктура на електронното управление""</div>
                                                <div class=""logo-text"" style=""padding-left:100px;"">Ако не желаете да получавате уведомления, натиснете {3}</div>
                                            </div>
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                            </div>
                            ";

        public const string CreateTemplate = @"
                            <div class=""col-md-offset-1"">
                            <p>Здравейте,  </p>
                            <p>Създадена е задача: <p> {0}</p> в която сте участник</p>
                            <p>За вход в системата, ползвайте домейнския си акаунт и парола</p>
                            </div>
                            <div>
                            <table>
                                <tbody>
                                    <tr>
                                        <td><img src=""{2}"" alt=""Useful alt text"" width=""70"" height=""100"" border=""0"" style=""border:0; outline:none; text-decoration:none; display:block;"">
                                        </td>
                                        <td style = ""padding-left:10px;"" >
                                            <div>
                                                <div class=""logo-text""><a href = ""{1}"">Мениджър задачи</a></div>
                                                <div class=""logo-text-secondary"">Изпълнителна агенция ""Инфраструктура на електронното управление""</div>
                                                <div class=""logo-text"" style=""padding-left:100px;"">Ако не желаете да получавате уведомления, натиснете {3}</div>
                                            </div>
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                            </div>
                            ";

        public const string AddColeaguesTemplate = @"
                            <div class=""col-md-offset-1"">
                            <p>Здравейте,  </p>
                            <p>Включен/а сте в задача: <p> {0}</p></p>
                            <p>За вход в системата, ползвайте домейнския си акаунт и парола</p>
                            </div>

                            <div>
                            <table>
                                <tbody>
                                    <tr>
                                        <td><img src=""{2}"" alt=""Useful alt text"" width=""70"" height=""100"" border=""0"" style=""border:0; outline:none; text-decoration:none; display:block;"">
                                        </td>
                                        <td style = ""padding-left:10px;"" >
                                            <div>
                                                <div class=""logo-text""><a href = ""{1}"">Мениджър задачи</a></div>
                                                <div class=""logo-text-secondary"">Изпълнителна агенция ""Инфраструктура на електронното управление""</div>
                                                <div class=""logo-text"" style=""padding-left:100px;"">Ако не желаете да получавате уведомления, натиснете {3}</div>
                                            </div>
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                            </div>
                            ";


        public const string CloseTemplate = @"
                            <div class=""col-md-offset-1"">
                            <p>Здравейте,  </p>
                            <p>Задача: <p> {0}</p> в която сте участник е приключена</p>
                            <p>За вход в системата, ползвайте домейнския си акаунт и парола</p>
                            </div>

                            <div>
                            <table>
                                <tbody>
                                    <tr>
                                        <td><img src=""{2}"" alt=""Useful alt text"" width=""70"" height=""100"" border=""0"" style=""border:0; outline:none; text-decoration:none; display:block;"">
                                        </td>
                                        <td style = ""padding-left:10px;"" >
                                            <div>
                                                <div class=""logo-text""><a href = ""{1}"">Мениджър задачи</a></div>
                                                <div class=""logo-text-secondary"">Изпълнителна агенция ""Инфраструктура на електронното управление""</div>
                                                <div class=""logo-text"" style=""padding-left:100px;"">Ако не желаете да получавате уведомления, натиснете {3}</div>
                                            </div>
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                            </div>
                            ";



        private static readonly Dictionary<string, string> MIMETypesDictionary = new Dictionary<string, string>
  {
    {"ai", "application/postscript"},
    {"aif", "audio/x-aiff"},
    {"aifc", "audio/x-aiff"},
    {"aiff", "audio/x-aiff"},
    {"asc", "text/plain"},
    {"atom", "application/atom+xml"},
    {"au", "audio/basic"},
    {"avi", "video/x-msvideo"},
    {"bcpio", "application/x-bcpio"},
    {"bin", "application/octet-stream"},
    {"bmp", "image/bmp"},
    {"cdf", "application/x-netcdf"},
    {"cgm", "image/cgm"},
    {"class", "application/octet-stream"},
    {"cpio", "application/x-cpio"},
    {"cpt", "application/mac-compactpro"},
    {"csh", "application/x-csh"},
    {"css", "text/css"},
    {"dcr", "application/x-director"},
    {"dif", "video/x-dv"},
    {"dir", "application/x-director"},
    {"djv", "image/vnd.djvu"},
    {"djvu", "image/vnd.djvu"},
    {"dll", "application/octet-stream"},
    {"dmg", "application/octet-stream"},
    {"dms", "application/octet-stream"},
    {"doc", "application/msword"},
    {"docx","application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
    {"dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template"},
    {"docm","application/vnd.ms-word.document.macroEnabled.12"},
    {"dotm","application/vnd.ms-word.template.macroEnabled.12"},
    {"dtd", "application/xml-dtd"},
    {"dv", "video/x-dv"},
    {"dvi", "application/x-dvi"},
    {"dxr", "application/x-director"},
    {"eps", "application/postscript"},
    {"etx", "text/x-setext"},
    {"exe", "application/octet-stream"},
    {"ez", "application/andrew-inset"},
    {"gif", "image/gif"},
    {"gram", "application/srgs"},
    {"grxml", "application/srgs+xml"},
    {"gtar", "application/x-gtar"},
    {"hdf", "application/x-hdf"},
    {"hqx", "application/mac-binhex40"},
    {"htm", "text/html"},
    {"html", "text/html"},
    {"ice", "x-conference/x-cooltalk"},
    {"ico", "image/x-icon"},
    {"ics", "text/calendar"},
    {"ief", "image/ief"},
    {"ifb", "text/calendar"},
    {"iges", "model/iges"},
    {"igs", "model/iges"},
    {"jnlp", "application/x-java-jnlp-file"},
    {"jp2", "image/jp2"},
    {"jpe", "image/jpeg"},
    {"jpeg", "image/jpeg"},
    {"jpg", "image/jpeg"},
    {"js", "application/x-javascript"},
    {"kar", "audio/midi"},
    {"latex", "application/x-latex"},
    {"lha", "application/octet-stream"},
    {"lzh", "application/octet-stream"},
    {"m3u", "audio/x-mpegurl"},
    {"m4a", "audio/mp4a-latm"},
    {"m4b", "audio/mp4a-latm"},
    {"m4p", "audio/mp4a-latm"},
    {"m4u", "video/vnd.mpegurl"},
    {"m4v", "video/x-m4v"},
    {"mac", "image/x-macpaint"},
    {"man", "application/x-troff-man"},
    {"mathml", "application/mathml+xml"},
    {"me", "application/x-troff-me"},
    {"mesh", "model/mesh"},
    {"mid", "audio/midi"},
    {"midi", "audio/midi"},
    {"mif", "application/vnd.mif"},
    {"mov", "video/quicktime"},
    {"movie", "video/x-sgi-movie"},
    {"mp2", "audio/mpeg"},
    {"mp3", "audio/mpeg"},
    {"mp4", "video/mp4"},
    {"mpe", "video/mpeg"},
    {"mpeg", "video/mpeg"},
    {"mpg", "video/mpeg"},
    {"mpga", "audio/mpeg"},
    {"ms", "application/x-troff-ms"},
    {"msh", "model/mesh"},
    {"mxu", "video/vnd.mpegurl"},
    {"nc", "application/x-netcdf"},
    {"oda", "application/oda"},
    {"ogg", "application/ogg"},
    {"pbm", "image/x-portable-bitmap"},
    {"pct", "image/pict"},
    {"pdb", "chemical/x-pdb"},
    {"pdf", "application/pdf"},
    {"pgm", "image/x-portable-graymap"},
    {"pgn", "application/x-chess-pgn"},
    {"pic", "image/pict"},
    {"pict", "image/pict"},
    {"png", "image/png"},
    {"pnm", "image/x-portable-anymap"},
    {"pnt", "image/x-macpaint"},
    {"pntg", "image/x-macpaint"},
    {"ppm", "image/x-portable-pixmap"},
    {"ppt", "application/vnd.ms-powerpoint"},
    {"pptx","application/vnd.openxmlformats-officedocument.presentationml.presentation"},
    {"potx","application/vnd.openxmlformats-officedocument.presentationml.template"},
    {"ppsx","application/vnd.openxmlformats-officedocument.presentationml.slideshow"},
    {"ppam","application/vnd.ms-powerpoint.addin.macroEnabled.12"},
    {"pptm","application/vnd.ms-powerpoint.presentation.macroEnabled.12"},
    {"potm","application/vnd.ms-powerpoint.template.macroEnabled.12"},
    {"ppsm","application/vnd.ms-powerpoint.slideshow.macroEnabled.12"},
    {"ps", "application/postscript"},
    {"qt", "video/quicktime"},
    {"qti", "image/x-quicktime"},
    {"qtif", "image/x-quicktime"},
    {"ra", "audio/x-pn-realaudio"},
    {"ram", "audio/x-pn-realaudio"},
    {"ras", "image/x-cmu-raster"},
    {"rdf", "application/rdf+xml"},
    {"rgb", "image/x-rgb"},
    {"rm", "application/vnd.rn-realmedia"},
    {"roff", "application/x-troff"},
    {"rtf", "text/rtf"},
    {"rtx", "text/richtext"},
    {"sgm", "text/sgml"},
    {"sgml", "text/sgml"},
    {"sh", "application/x-sh"},
    {"shar", "application/x-shar"},
    {"silo", "model/mesh"},
    {"sit", "application/x-stuffit"},
    {"skd", "application/x-koan"},
    {"skm", "application/x-koan"},
    {"skp", "application/x-koan"},
    {"skt", "application/x-koan"},
    {"smi", "application/smil"},
    {"smil", "application/smil"},
    {"snd", "audio/basic"},
    {"so", "application/octet-stream"},
    {"spl", "application/x-futuresplash"},
    {"src", "application/x-wais-source"},
    {"sv4cpio", "application/x-sv4cpio"},
    {"sv4crc", "application/x-sv4crc"},
    {"svg", "image/svg+xml"},
    {"swf", "application/x-shockwave-flash"},
    {"t", "application/x-troff"},
    {"tar", "application/x-tar"},
    {"tcl", "application/x-tcl"},
    {"tex", "application/x-tex"},
    {"texi", "application/x-texinfo"},
    {"texinfo", "application/x-texinfo"},
    {"tif", "image/tiff"},
    {"tiff", "image/tiff"},
    {"tr", "application/x-troff"},
    {"tsv", "text/tab-separated-values"},
    {"txt", "text/plain"},
    {"ustar", "application/x-ustar"},
    {"vcd", "application/x-cdlink"},
    {"vrml", "model/vrml"},
    {"vxml", "application/voicexml+xml"},
    {"wav", "audio/x-wav"},
    {"wbmp", "image/vnd.wap.wbmp"},
    {"wbmxl", "application/vnd.wap.wbxml"},
    {"wml", "text/vnd.wap.wml"},
    {"wmlc", "application/vnd.wap.wmlc"},
    {"wmls", "text/vnd.wap.wmlscript"},
    {"wmlsc", "application/vnd.wap.wmlscriptc"},
    {"wrl", "model/vrml"},
    {"xbm", "image/x-xbitmap"},
    {"xht", "application/xhtml+xml"},
    {"xhtml", "application/xhtml+xml"},
    {"xls", "application/vnd.ms-excel"},
    {"xml", "application/xml"},
    {"xpm", "image/x-xpixmap"},
    {"xsl", "application/xml"},
    {"xlsx","application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
    {"xltx","application/vnd.openxmlformats-officedocument.spreadsheetml.template"},
    {"xlsm","application/vnd.ms-excel.sheet.macroEnabled.12"},
    {"xltm","application/vnd.ms-excel.template.macroEnabled.12"},
    {"xlam","application/vnd.ms-excel.addin.macroEnabled.12"},
    {"xlsb","application/vnd.ms-excel.sheet.binary.macroEnabled.12"},
    {"xslt", "application/xslt+xml"},
    {"xul", "application/vnd.mozilla.xul+xml"},
    {"xwd", "image/x-xwindowdump"},
    {"xyz", "chemical/x-xyz"},
    {"zip", "application/zip"}
  };

    public static string GetMIMEType(string fileName)
    {
        //get file extension
        string extension = Path.GetExtension(fileName).ToLowerInvariant();

        if (extension.Length > 0 &&
            MIMETypesDictionary.ContainsKey(extension.Remove(0, 1)))
        {
            return MIMETypesDictionary[extension.Remove(0, 1)];
        }
        return "unknown/unknown";
    }

}


}