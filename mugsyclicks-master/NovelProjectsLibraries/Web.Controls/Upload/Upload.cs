using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Hosting;

namespace NovelProjects.Web
{
    [ToolboxData("<{0}:Upload runat=server></{0}:Upload>")]
    [Serializable]
    public class Upload : WebControl
    {
        #region control properties
        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        #endregion

        #region private variables
        private PlaceHolder ph = new PlaceHolder();
        private LiteralControl lt;

        private Boolean SetViewState = true;

        private String _UploadURL = "upload.aspx";
        private String _SuccessURL = null;
        private String _CancelURL = null;
        private String _BasicURL = null;

        private String _FileTypeDescription = "All files";
        private String _FileTypeExtensions = "*";

        private Int32 _MinImageWidth = -1;
        private Int32 _MinImageHeight = -1;
        private Boolean _EnforceDimensions = false;

        private Int32 _MinFileSize = 0;
        private Boolean _EnforceMinSize = false;
        private Int32 _MaxFileSize = 1073741824;
        private Boolean _EnforceMaxSize = true;

        private Boolean _AutoUpload = false;

        private String _UploadButtonInitialText = "Browse files";
        private String _UploadButtonAddMoreText = "Add more files";
        private String _UploadButtonSrc = null;
        private int _UploadButtonWidth = 250;
        private int _UploadButtonHeight = 40;
        #endregion

        #region public variables
        public String UploadURL
        {
            get
            {
                return SetViewState && ViewState["UploadURL"] != null ? ViewState["UploadURL"].ToString() : _UploadURL;
            }
            set
            {
                _UploadURL = value;
                ViewState["UploadURL"] = _UploadURL;
            }
        }

        public String SuccessURL
        {
            get
            {
                return SetViewState && ViewState["SuccessURL"] != null ? ViewState["SuccessURL"].ToString() : _SuccessURL;
            }
            set
            {
                _SuccessURL = value;
                ViewState["SuccessURL"] = _SuccessURL;
            }
        }

        public String CancelURL
        {
            get
            {
                return SetViewState && ViewState["CancelURL"] != null ? ViewState["CancelURL"].ToString() : _CancelURL;
            }
            set
            {
                _CancelURL = value;
                ViewState["CancelURL"] = _CancelURL;
            }
        }

        public String BasicURL
        {
            get
            {
                return SetViewState && ViewState["BasicURL"] != null ? ViewState["BasicURL"].ToString() : _BasicURL;
            }
            set
            {
                _BasicURL = value;
                ViewState["BasicURL"] = _BasicURL;
            }
        }

        public String FileTypeDescription
        {
            get
            {
                return _FileTypeDescription;
            }
            set
            {
                _FileTypeDescription = value;
            }
        }

        public String FileTypeExtensions
        {
            get
            {
                return _FileTypeExtensions;
            }
            set
            {
                _FileTypeExtensions = value;
            }
        }

        public Int32 MinImageWidth
        {
            get
            {
                return _MinImageWidth;
            }
            set
            {
                if (value > 0 && value < Int32.MaxValue)
                    _MinImageWidth = value;
            }
        }

        public Int32 MinImageHeight
        {
            get
            {
                return _MinImageHeight;
            }
            set
            {
                if (value > 0 && value < Int32.MaxValue)
                    _MinImageHeight = value;
            }
        }

        public Boolean EnforceDimensions
        {
            get
            {
                return _EnforceDimensions;
            }
            set
            {
                _EnforceDimensions = value;
            }
        }

        public Int32 MinFileSize
        {
            get
            {
                return _MinFileSize;
            }
            set
            {
                if (value > 0 && value < Int32.MaxValue)
                    _MinFileSize = value;
            }
        }

        public Int32 MaxFileSize
        {
            get
            {
                return _MaxFileSize;
            }
            set
            {
                if (value > 0 && value < Int32.MaxValue)
                    _MaxFileSize = value;
            }
        }

        public Boolean EnforceMinSize
        {
            get
            {
                return _EnforceMinSize;
            }
            set
            {
                _EnforceMinSize = value;
            }
        }

        public Boolean EnforceMaxSize
        {
            get
            {
                return _EnforceMaxSize;
            }
            set
            {
                _EnforceMaxSize = value;
            }
        }

        public Boolean AutoUpload
        {
            get
            {
                return _AutoUpload;
            }
            set
            {
                _AutoUpload = value;
            }
        }

        public String UploadButtonInitialText
        {
            get
            {
                return _UploadButtonInitialText;
            }
            set
            {
                _UploadButtonInitialText = value;
            }
        }
        public String UploadButtonAddMoreText
        {
            get
            {
                return _UploadButtonAddMoreText;
            }
            set
            {
                _UploadButtonAddMoreText = value;
            }
        }
        public String UploadButtonSrc
        {
            get
            {
                return _UploadButtonSrc;
            }
            set
            {
                _UploadButtonSrc = value;
            }
        }
        public int UploadButtonWidth
        {
            get { return _UploadButtonWidth; }
            set { _UploadButtonWidth = value; }
        }
        public int UploadButtonHeight
        {
            get { return _UploadButtonHeight; }
            set { _UploadButtonHeight = value; }
        }
        #endregion

        #region Writes out the editable div or standard text
        protected override void RenderContents(HtmlTextWriter output)
        {
        }
        #endregion

        #region Initializes all of the controls
        protected override void OnInit(EventArgs args)
        {
            HostingEnvironment.RegisterVirtualPathProvider(new AssemblyResourceProvider());

            base.OnInit(args);
        }

        protected override void OnLoad(EventArgs args)
        {
            Int32 flashversion = (MinImageHeight > 0 && MinImageWidth > 0) ? 10 : 9;

            //Adds Javascript code/files and CSS files to page header
            lt = new LiteralControl();

            lt.Text += "<script type=\"text/javascript\">try { jQuery.support.boxModel != 'test' } catch (err) { document.write(unescape(\"%3Cscript src='" + Page.ClientScript.GetWebResourceUrl(typeof(Upload), "NovelProjects.Web.javascript.jquery-1.3.2.min.js") + "' type='text/javascript'%3E%3C/script%3E\")); }</script>\n";
            lt.Text += "<!--<script type=\"text/javascript\" src=\"" + Page.ClientScript.GetWebResourceUrl(typeof(Upload), "NovelProjects.Web.javascript.jquery-1.3.2.min.js") + "\" ></script>-->\n";
            lt.Text += "<script type=\"text/javascript\" src=\"" + Page.ClientScript.GetWebResourceUrl(typeof(Upload), "NovelProjects.Web.Upload.swfobject.min.js") + "\" ></script>\n";
            lt.Text += "<link rel=\"Stylesheet\" type=\"text/css\" href=\"" + Page.ClientScript.GetWebResourceUrl(typeof(Upload), "NovelProjects.Web.Upload.npUpload.css") + "\" />\n";
            lt.Text += "<script type=\"text/javascript\" src=\"" + Page.ClientScript.GetWebResourceUrl(typeof(Upload), "NovelProjects.Web.Upload.npUpload.min.js") + "\" ></script>\n";

            lt.Text += "<script type=\"text/javascript\">\n";
            lt.Text += "\t\tvar autoUpload = " + _AutoUpload.ToString().ToLower() + ";\n";
            lt.Text += "\t\tvar enforceMax = " + _EnforceMaxSize.ToString().ToLower() + ";\n";
            lt.Text += "\t\tvar enforceMin = " + _EnforceMinSize.ToString().ToLower() + ";\n";
            lt.Text += "\t\tvar enforceDim = " + _EnforceDimensions.ToString().ToLower() + ";\n";
            lt.Text += "\t\tvar flashvars = {\n";
            lt.Text += "\t\t\tuploadUrl: '" + UploadURL + "',\n";
            lt.Text += "\t\t\tmaxFileSize: " + _MaxFileSize + ",\n";
            lt.Text += "\t\t\tminFileSize: " + _MinFileSize + ",\n";
            lt.Text += "\t\t\tbrowseText: '" + _UploadButtonInitialText + "',\n";
            lt.Text += "\t\t\taddMoreText: '" + _UploadButtonAddMoreText + "',\n";

            if (MinImageHeight > 0 && MinImageWidth > 0)
            {
                lt.Text += "\t\t\tfileDescription: 'Images Only',\n";
                lt.Text += "\t\t\tfileExtension: '*.jpg;*.jpeg;*.png;*.gif',\n";
                lt.Text += "\t\t\tminImageWidth: " + MinImageWidth + ",\n";
                lt.Text += "\t\t\tminImageHeight: " + MinImageHeight + ",\n";
                lt.Text += "\t\t\tminImageDPI: 72\n";
            }
            else
            {
                lt.Text += "\t\t\tfileDescription: '" + _FileTypeDescription + "',\n";
                lt.Text += "\t\t\tfileExtension: '" + _FileTypeExtensions + "'\n";
            }

            lt.Text += "\t\t};\n";
            lt.Text += "\t\tvar params = { quality:'high', swliveconnect:'true', wmode:'transparent', play:'true', allowscriptaccess:'sameDomain', bgcolor:'#f6f6f6' };\n";
            lt.Text += "\t\tvar attributes = { id:'npMainFlashUpload', name:'npMainFlashUpload' };\n";

            if (flashversion == 9)
                lt.Text += "\t\tswfobject.embedSWF('" + Page.ClientScript.GetWebResourceUrl(typeof(Upload), "NovelProjects.Web.Upload.upload_v9.swf") + "', 'largeflashbutton','250', '40', '9.0.0', '" + Page.ClientScript.GetWebResourceUrl(typeof(Upload), "NovelProjects.Web.Upload.expressInstall.swf") + "', flashvars, params, attributes);\n";
            else
                lt.Text += "\t\tswfobject.embedSWF('" + Page.ClientScript.GetWebResourceUrl(typeof(Upload), "NovelProjects.Web.Upload.upload.swf") + "', 'largeflashbutton', '250', '40', '10.0.0', '" + Page.ClientScript.GetWebResourceUrl(typeof(Upload), "NovelProjects.Web.Upload.expressInstall.swf") + "', flashvars, params, attributes);\n";

            if (BasicURL != null)
            {
                lt.Text += "\n\tif (!swfobject.hasFlashPlayerVersion('" + flashversion + ".0.0')){\n";
                lt.Text += "\t\t$('#npUpload .basic').click();\n";
                lt.Text += "\t}\n";
            }

            lt.Text += "</script>\n";

            if (_UploadButtonSrc != null)
            {
                System.Drawing.Image button = null;

                try
                {
                    String folder = HttpContext.Current.Request.Url.AbsolutePath.Substring(0, HttpContext.Current.Request.Url.AbsolutePath.LastIndexOf('/') + 1);
                    String path = HttpContext.Current.Server.MapPath(folder + _UploadButtonSrc);
                    button = System.Drawing.Image.FromFile(path);

                    lt.Text += "<style type=\"text/css\">\n";
                    lt.Text += "\t#npUpload .uploadbtn { background:url(" + _UploadButtonSrc + ") no-repeat; width:" + button.Width + "px; height:" + button.Height + "px; }\n";
                    lt.Text += "</style>\n";

                    button.Dispose();
                }
                catch
                {
                    if (button != null) button.Dispose();
                }
            }

            Page.Header.Controls.Add(lt);
            BuildUpload();
            Controls.Add(ph);

            base.OnLoad(args);
        }
        #endregion

        #region Renders all of the controls in the page
        protected override void Render(HtmlTextWriter writer)
        {
            EnsureChildControls();

            //Render the main controls on the page
            ph.RenderControl(writer);

            base.Render(writer);
        }
        #endregion

        #region Build Upload control
        private void BuildUpload()
        {
            lt = new LiteralControl();

            lt.Text += "\n<div id=\"npUpload\">";
            lt.Text += "\n\t<ul class=\"steps\">";
            lt.Text += "\n\t\t<li>";
            lt.Text += "\n\t\t\t<h2 class=\"step\">Step 1:</h2>";
            lt.Text += "\n\t\t\t<h1 id=\"largeflashbutton\">Choose files</h1>";
            lt.Text += "\n\t\t</li>";
            lt.Text += "\n\t\t<li>";
            lt.Text += "\n\t\t\t<h2 class=\"step\">Step 2:</h2>";
            lt.Text += "\n\t\t\t<h1 class=\"nextstep\">Confirm files for upload</h1>";
            lt.Text += "\n\t\t\t<div class=\"filetop hide\">";
            lt.Text += "\n\t\t\t\t<div class=\"name\">Filename</div>";
            lt.Text += "\n\t\t\t\t<div class=\"size\">Size</div>";
            lt.Text += "\n\t\t\t\t<div class=\"delete\">Remove?</div>";
            lt.Text += "\n\t\t\t\t<div class=\"clear\"></div>";
            lt.Text += "\n\t\t\t</div>";
            lt.Text += "\n\t\t\t<div class=\"files hide\"></div>";
            lt.Text += "\n\t\t\t<div class=\"filefooter hide\">";
            lt.Text += "\n\t\t\t\t<div class=\"files-more\">";
            lt.Text += "\n\t\t\t\t\t<span class=\"totalfiles\"><span class=\"count\">0</span> files</span>";
            lt.Text += "\n\t\t\t\t\t<span id=\"smallflashbutton\"></span>";
            lt.Text += "\n\t\t\t\t</div>";
            lt.Text += "\n\t\t\t\t<div class=\"tsize\">Total: <span class=\"totalsize\"></span></div>";
            lt.Text += "\n\t\t\t\t<div class=\"clear\"></div>";
            lt.Text += "\n\t\t\t</div>";
            lt.Text += "\n\t\t</li>";
            lt.Text += "\n\t\t<li>";
            lt.Text += "\n\t\t\t<h2 class=\"step\">Step 3:</h2>";
            lt.Text += "\n\t\t\t<h1 class=\"nextstep\">Upload files</h1>";
            lt.Text += "\n\t\t\t<a class=\"uploadbtn hide\"><span>Upload files</span></a>";

            if (CancelURL != null)
                lt.Text += "\n\t\t\t<span class=\"cancel hide\">Or, <a href=\"" + CancelURL + "\">cancel</a> and go back</span>";

            lt.Text += "\n\t\t</li>";
            lt.Text += "\n\t</ul>";

            if (BasicURL != null)
                lt.Text += "\n\t<p class=\"basic\">looking for the <a href=\"" + BasicURL + "\">basic uploader</a>?</p>";

            lt.Text += "\n\t<div class=\"controls\">\n\t\t";
            ph.Controls.Add(lt);

            Button b1 = new Button
            {
                CausesValidation = false,
                OnClientClick = "location.href = '" + (BasicURL ?? HttpContext.Current.Request.Url.AbsoluteUri) + "'; return false;",
                CssClass = "basic",
                Text = "basic"
            };
            ph.Controls.Add(b1);

            lt = new LiteralControl();
            lt.Text += "\n\t\t<!-- basic url: " + BasicURL + " -->\n\t\t";
            ph.Controls.Add(lt);

            Button b2 = new Button
            {
                CausesValidation = false,
                OnClientClick = "location.href = '" + (SuccessURL ?? HttpContext.Current.Request.Url.AbsoluteUri) + "'; return false;",
                CssClass = "redirect",
                Text = "redirect"
            };
            ph.Controls.Add(b2);

            lt = new LiteralControl();
            lt.Text += "\n\t\t<!-- redirect url: " + SuccessURL + " -->\n\t\t";
            lt.Text += "\n\t</div>";
            lt.Text += "\n</div>";
            ph.Controls.Add(lt);
        }
        #endregion

        #region Click Events
        protected void Basic_Click(Object sender, EventArgs args)
        {
            if (BasicURL != null)
                HttpContext.Current.Response.Redirect(BasicURL);
            else
                HttpContext.Current.Response.Redirect(HttpContext.Current.Request.Url.AbsoluteUri);
        }

        protected void Redirect_Click(Object sender, EventArgs args)
        {
            if (SuccessURL != null)
                HttpContext.Current.Response.Redirect(SuccessURL);
            else
                HttpContext.Current.Response.Redirect(HttpContext.Current.Request.Url.AbsoluteUri);
        }
        #endregion
    }
}