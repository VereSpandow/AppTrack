using System.Web;
using System.Web.Optimization;

namespace AppTrack
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-2.6.2.js"));

            bundles.Add(new ScriptBundle("~/bundles/ie").Include(
                      "~/Scripts/html5shiv.min.js",
                      "~/Scripts/respond.min.js"));

             bundles.Add(new StyleBundle("~/Content/css").Include(
                                  "~/Content/bootstrap.css",
                                  "~/Content/font-awesome.min.css",
                                  "~/Content/loyaltybenefits.css"
                                  ));

             bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                           "~/Content/themes/base/jquery-ui.css"));

             /*     
                        bundles.Add(new StyleBundle("~/Content/css").Include(
                            "~/Content/bootstrap.min.css",
                            "~/Content/font-awesome.min.css",
                            "~/Content/simple-line-icons.css",
                            "~/Content/Sidebar.css",
                            "~/Content/animate.css",
                            "~/Content/main.css"));
             */

             bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
               "~/Scripts/jquery-2.1.3.min.js"));

             bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
             "~/Scripts/jquery-ui-1.11.4.min.js",
             "~/Scripts/jquery-ui.unobtrusive-2.2.0.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                "~/Scripts/jquery.validate.min.js",
                "~/Scripts/jquery.validate.unobtrusive*",
                "~/Scripts/jquery.maskedinput*",
                "~/Scripts/jquery.unobtrusive-ajax.min.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/bootstrap.min.js"));

            /*            bundles.Add(new ScriptBundle("~/plugins/pace").Include(
                        "~/Scripts/plugins/pace/pace.min.js"));
                        bundles.Add(new ScriptBundle("~/plugins/navgoco").Include(
                        "~/Scripts/plugins/navgoco/jquery.navgoco.min.js"));

                        bundles.Add(new ScriptBundle("~/bundles/app").Include(
                       "~/Scripts/src/app.js"));
            */



            // Set EnableOptimizations to false for debugging. For more information,
            // visit http://go.microsoft.com/fwlink/?LinkId=301862
            BundleTable.EnableOptimizations = false;
        
        }
    }
}
