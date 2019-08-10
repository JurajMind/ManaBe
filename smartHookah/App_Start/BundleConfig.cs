using System.Web.Optimization;

namespace smartHookah
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Clear();
            bundles.ResetAll();
            //BundleTable.EnableOptimizations = true;

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/jquery.unobtrusive-ajax.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/main").Include(
             "~/Scripts/main.js"
              ));

            bundles.Add(new ScriptBundle("~/bundles/slim").Include(
                "~/Scripts/slim/*.js"));

            bundles.Add(new ScriptBundle("~/bundles/shake").Include(
                "~/Scripts/shake.js"));

            bundles.Add(new ScriptBundle("~/bundles/gij").Include(
                "~/Scripts/gijgo/combined/gijgo.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/jquery-rating").Include(
                "~/Scripts/bootstrap-rating-input.js"));

            bundles.Add(new ScriptBundle("~/bundles/vue").Include(
                "~/Scripts/vue.js"));

            bundles.Add(new ScriptBundle("~/bundles/babel").Include(
                "~/Scripts/babel.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/copyjs").Include(
                "~/Scripts/clipboard.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/wizard").Include(
                "~/Scripts/jquery.bootstrap.wizard.js",
                "~/Scripts/material-bootstrap-wizard.js"));

            bundles.Add(new ScriptBundle("~/bundles/datepicker").Include(
                "~/Scripts/bootstrap-material-datetimepicker.js"));

            bundles.Add(new ScriptBundle("~/bundles/qrReader").Include(
                "~/Scripts/webqr.js",
                "~/Scripts/llqrcode.js"));

            bundles.Add(new ScriptBundle("~/bundles/chart").Include(
                "~/Scripts/smoothie.js",
                "~/Scripts/Chart.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                //"~/Scripts/popper.js",
                //"~/Scripts/bootstrap.js",
                "~/Scripts/bootstrap-tagsinput.js",
                //"~/Scripts/respond.js",
                "~/Scripts/jquery.stayInWebApp.js",
                "~/Scripts/jquery.wheelcolorpicker.js",
                "~/Scripts/bootstrap-modalmanager.js"));

            bundles.Add(new ScriptBundle("~/bundles/signalr").Include(
                "~/Scripts/jquery.signalR-2.4.1.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/moment").Include(
                "~/Scripts/moment-with-locales.js",
                "~/Scripts/moment-duration-format.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/controll").Include(
                "~/Scripts/nouislider.js"));

            bundles.Add(
                new ScriptBundle("~/bundles/highcharts").Include("~/Scripts/highcharts.js")
                    .IncludeDirectory("~/Scripts/highcharts", "*.js", true));

            bundles.Add(new ScriptBundle("~/bundles/material").Include(
                "~/Scripts/tether.js",
                "~/Scripts/arrive.min.js",
                "~/Scripts/bootstrap-material-design.iife.js",
                "~/Scripts/snackbar.js",
                "~/Scripts/ie10-viewport-bug-workaround.js"
              ));

            bundles.Add(new ScriptBundle("~/bundles/phaser").Include(
                "~/Scripts/phaser.js"));

            bundles.Add(new StyleBundle("~/Content/datepicker").Include(
                "~/Content/bootstrap-material-datetimepicker.css"));

            bundles.Add(new StyleBundle("~/Content/wizard").Include(
                "~/Content/material-bootstrap-wizard.css"));

            bundles.Add(new StyleBundle("~/Content/material").Include(
                "~/Content/bootstrap-material-design.css",
                "~/Content/PagedList.css",
                "~/Content/noUiSlider/nouislider.min.css",
                "~/Content/bootstrap-fs-modal.css"
           ));

            bundles.Add(new StyleBundle("~/Content/season").Include(
                "~/Content/season.css"));


            bundles.Add(new StyleBundle("~/Content/gij").Include(
                "~/Content/gijgo/combined/gijgo.css"));


            bundles.Add(new StyleBundle("~/Content/slim").Include(
                "~/Content/slim.min.css"));

            bundles.Add(new StyleBundle("~/Content/font-awesome")
                .Include("~/Content/font-awesome.css", new CssRewriteUrlTransform()));

            bundles.Add(new StyleBundle("~/Content/critical")
                .Include("~/Content/critical.css", new CssRewriteUrlTransform()));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                    "~/Content/bootstrap-tagsinput.css",
                    "~/Content/bmd.css"
                    ).Include("~/Content/wheelcolorpicker.css",new CssRewriteUrlTransform()).Include("~/Content/site.css",new CssRewriteUrlTransform()));

            bundles.Add(new StyleBundle("~/Content/cssAll").Include(
  
                "~/Content/PagedList.css",
                "~/Content/noUiSlider/nouislider.min.css",
                "~/Content/bootstrap-material-design.css",
                "~/Content/bootstrap-tagsinput.css"
            ).Include("~/Content/wheelcolorpicker.css").Include("~/Content/font-awesome.css", new CssRewriteUrlTransform())
            .Include("~/Content/bmd.css")
            .Include("~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content").Include(
                "~/Content/range-sliders.css"));

            bundles.Add(new ScriptBundle("~/Scripts").Include(
                "~/Scripts/global.js"));


        }
    }
}