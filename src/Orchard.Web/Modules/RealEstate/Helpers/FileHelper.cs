using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Mvc;

namespace RealEstate.Helpers
{
    public class FileHelper
    {
        public static bool FileExits(string filename)
        {
            return File.Exists(filename);
        }

        public static DateTime LastestUpdate(string filename)
        {
            return File.Exists(filename) ? File.GetLastWriteTime(filename) : DateTime.Now.AddYears(-1);
        }

        public static void DeleteFile(string filename)
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
        }

        public static void DeleteDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        public static void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static void WriteFileInUtf8(string filename, string content)
        {
            string filepath = Path.GetDirectoryName(filename);
            CreateDirectory(filepath);

            DeleteFile(filename);
            var fs = new FileStream(filename, FileMode.Create);

            var streamW = new StreamWriter(fs, Encoding.UTF8);
            streamW.Write(content);

            streamW.Flush();
            streamW.Close();
        }

        public static void WriteFileInUtf8(string filename, List<string> content)
        {
            string filepath = Path.GetDirectoryName(filename);
            CreateDirectory(filepath);

            DeleteFile(filename);
            var fs = new FileStream(filename, FileMode.Create);
            var streamW = new StreamWriter(fs, Encoding.UTF8);

            foreach (string line in content)
            {
                streamW.Write(line + "\n");
            }

            streamW.Flush();
            streamW.Close();
        }

        public static string ReadFileInUtf8(string filename)
        {
            try
            {
                if (FileExits(filename))
                    return File.ReadAllText(filename, Encoding.UTF8);
            }
            catch
            {
                return null;
            }

            return null;
        }
    }

    public static class ControllerExtensions
    {
        public static string RenderView(this Controller controller, string viewName, object model)
        {
            return RenderView(controller, viewName, new ViewDataDictionary(model));
        }

        private static string RenderView(this Controller controller, string viewName, ViewDataDictionary viewData)
        {
            ControllerContext controllerContext = controller.ControllerContext;

            ViewEngineResult viewResult = ViewEngines.Engines.FindView(controllerContext, viewName, null);

            StringWriter stringWriter;

            using (stringWriter = new StringWriter())
            {
                var viewContext = new ViewContext(
                    controllerContext,
                    viewResult.View,
                    viewData,
                    controllerContext.Controller.TempData,
                    stringWriter);

                viewResult.View.Render(viewContext, stringWriter);
                viewResult.ViewEngine.ReleaseView(controllerContext, viewResult.View);
            }

            return stringWriter.ToString();
        }
    }
}