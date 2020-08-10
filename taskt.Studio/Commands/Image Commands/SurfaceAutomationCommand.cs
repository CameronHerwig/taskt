using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security;
using System.Windows.Forms;
using System.Xml.Serialization;
using taskt.Core.Attributes.ClassAttributes;
using taskt.Core.Attributes.PropertyAttributes;
using taskt.Core.Command;
using taskt.Core.Common;
using taskt.Core.Enums;
using taskt.Core.Infrastructure;
using taskt.Core.Utilities.CommandUtilities;
using taskt.Core.Utilities.CommonUtilities;
using taskt.Engine;
using taskt.UI.CustomControls;
using taskt.UI.CustomControls.CustomUIControls;
using taskt.UI.Forms.Supplement_Forms;

namespace taskt.Commands
{
    [Serializable]
    [Group("Image Commands")]
    [Description("This command attempts to find and perform an action on an existing image on screen.")]
    public class SurfaceAutomationCommand : ScriptCommand
    {
        [XmlAttribute]
        [PropertyDescription("Capture Search Image")]
        [InputSpecification("Use the tool to capture an image.")]
        [SampleUsage("")]
        [Remarks("The image will be used as the image to be found on screen.")]
        [PropertyUIHelper(UIAdditionalHelperType.ShowImageRecogitionHelper)]
        public string v_ImageCapture { get; set; }

        [XmlElement]
        [PropertyDescription("Element Action")]
        [PropertyUISelectionOption("Click Image")]
        [PropertyUISelectionOption("Set Text")]
        [PropertyUISelectionOption("Set Secure Text")]
        [PropertyUISelectionOption("Check If Image Exists")]
        [PropertyUISelectionOption("Wait For Image To Exist")]
        [InputSpecification("Select the appropriate corresponding action to take once the image has been located.")]
        [SampleUsage("")]
        [Remarks("Selecting this field changes the parameters required in the following step.")]
        public string v_ImageAction { get; set; }

        [XmlElement]
        [PropertyDescription("Additional Parameters")]
        [InputSpecification("Additional Parameters will be required based on the action settings selected.")]
        [SampleUsage("data || {vData} || *Variable Name*: vNewVariable")]
        [Remarks("Additional Parameters range from adding offset coordinates to specifying a variable to apply element text to.")]
        [PropertyUIHelper(UIAdditionalHelperType.ShowVariableHelper)]
        public DataTable v_ImageActionParameterTable { get; set; }

        [XmlAttribute]
        [PropertyDescription("Timeout (seconds)")]
        [InputSpecification("Enter a timeout length if required. Use 0 for unlimited search time.")]
        [SampleUsage("0 || 30 || ")]
        [Remarks("Search times become excessive for colors such as white. For best results, capture a large color variance on screen, not just a white block.")]
        [PropertyUIHelper(UIAdditionalHelperType.ShowVariableHelper)]
        public string v_TimeoutSeconds { get; set; }

        [XmlAttribute]
        [PropertyDescription("Accuracy (0-1)")]
        [InputSpecification("Enter a timeout length if required. Use 0 for unlimited search time.")]
        [SampleUsage("0.8 || 1 || {vAccuracy}")]
        [Remarks("Accuracy must be a value between 0 and 1")]
        [PropertyUIHelper(UIAdditionalHelperType.ShowVariableHelper)]
        public string v_MatchAccuracy { get; set; }

        public bool TestMode = false;

        [XmlIgnore]
        [NonSerialized]
        private DataGridView _imageGridViewHelper;

        [XmlIgnore]
        [NonSerialized]
        private ComboBox _imageActionDropdown;

        [XmlIgnore]
        [NonSerialized]
        private List<Control> _imageParameterControls;

        public SurfaceAutomationCommand()
        {
            CommandName = "SurfaceAutomationCommand";
            SelectionName = "Surface Automation";
            CommandEnabled = true;
            CustomRendering = true;
           
            v_TimeoutSeconds = "0";
            v_MatchAccuracy = "0.8";

            v_ImageActionParameterTable = new DataTable
            {
                TableName = "ImageActionParamTable" + DateTime.Now.ToString("MMddyy.hhmmss")
            };
            v_ImageActionParameterTable.Columns.Add("Parameter Name");
            v_ImageActionParameterTable.Columns.Add("Parameter Value");

            _imageGridViewHelper = new DataGridView();
            _imageGridViewHelper.AllowUserToAddRows = true;
            _imageGridViewHelper.AllowUserToDeleteRows = true;
            _imageGridViewHelper.Size = new Size(400, 250);
            _imageGridViewHelper.ColumnHeadersHeight = 30;
            _imageGridViewHelper.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _imageGridViewHelper.DataBindings.Add("DataSource", this, "v_ImageActionParameterTable", false, DataSourceUpdateMode.OnPropertyChanged);
            _imageGridViewHelper.AllowUserToAddRows = false;
            _imageGridViewHelper.AllowUserToDeleteRows = false;
            _imageGridViewHelper.AllowUserToResizeRows = false;
        }

        public override void RunCommand(object sender)
        {
            var engine = (AutomationEngineInstance)sender;
            bool testMode = TestMode;

            double vTimeout = double.Parse(v_TimeoutSeconds.ConvertToUserVariable(engine));
            double vSelectedAccuracy = double.Parse(v_MatchAccuracy.ConvertToUserVariable(engine));

            if (vSelectedAccuracy < 0 || vSelectedAccuracy > 1)
                throw new ArgumentOutOfRangeException("Accuracy is not a value between 0 and 1.");

            //user image to bitmap
            Bitmap userImage = new Bitmap(Common.Base64ToImage(v_ImageCapture));

            CommandControls.HideAllForms();
            Bitmap desktopImage = ImageMethods.Screenshot();
            Bitmap desktopOutput = new Bitmap(desktopImage);

            //get graphics for drawing on output file
            Graphics screenShotUpdate = Graphics.FromImage(desktopOutput);

            //declare maximum boundaries
            int userImageMaxWidth = userImage.Width - 1;
            int userImageMaxHeight = userImage.Height - 1;
            int desktopImageMaxWidth = desktopImage.Width - 1;
            int desktopImageMaxHeight = desktopImage.Height - 1;

            //create desktopOutput file
            Bitmap sampleOut = new Bitmap(userImage);

            //get graphics for drawing on output file
            Graphics sampleUpdate = Graphics.FromImage(sampleOut);

            List<ImageRecognitionFingerPrint> uniqueFingerprint = new List<ImageRecognitionFingerPrint>();
            Color lastcolor = Color.Transparent;

            //create fingerprint
            var pixelDensity = (userImage.Width * userImage.Height);

            int iteration = 0;
            Random random = new Random();
            while ((uniqueFingerprint.Count() < 20) && (iteration < pixelDensity))
            {
                int x = random.Next(userImage.Width);
                int y = random.Next(userImage.Height);
                Color color = sampleOut.GetPixel(x, y);

                if ((lastcolor != color) && (!uniqueFingerprint.Any(f => f.XLocation == x && f.YLocation == y)))
                {
                    uniqueFingerprint.Add(new ImageRecognitionFingerPrint() { PixelColor = color, XLocation = x, YLocation = y });
                    sampleUpdate.DrawRectangle(Pens.Yellow, x, y, 1, 1);
                }
                iteration++;
            }

            //begin search
            DateTime timeoutDue = DateTime.Now.AddSeconds(vTimeout);

            bool imageFound = false;
            //for each row on the screen
            for (int rowPixel = 0; rowPixel < desktopImage.Height - 1; rowPixel++)
            {
                if (rowPixel + uniqueFingerprint.First().YLocation >= desktopImage.Height)
                    continue;

                //for each column on screen
                for (int columnPixel = 0; columnPixel < desktopImage.Width - 1; columnPixel++)
                {
                    if ((vTimeout > 0) && (DateTime.Now > timeoutDue))
                    {
                        CommandControls.ShowAllForms();
                        throw new Exception("Image recognition command ran out of time searching for image");
                    }

                    if (columnPixel + uniqueFingerprint.First().XLocation >= desktopImage.Width)
                        continue;

                    try
                    {
                        //get the current pixel from current row and column
                        // userImageFingerPrint.First() for now will always be from top left (0,0)
                        var currentPixel = desktopImage.GetPixel(columnPixel + uniqueFingerprint.First().XLocation, rowPixel + uniqueFingerprint.First().YLocation);

                        //compare to see if desktop pixel matches top left pixel from user image
                        if (currentPixel == uniqueFingerprint.First().PixelColor)
                        {
                            //look through each item in the fingerprint to see if offset pixel colors match
                            int matchCount = 0;
                            for (int item = 0; item < uniqueFingerprint.Count; item++)
                            {
                                //find pixel color from offset X,Y relative to current position of row and column
                                currentPixel = desktopImage.GetPixel(columnPixel + uniqueFingerprint[item].XLocation, rowPixel + uniqueFingerprint[item].YLocation);

                                //if color matches
                                if (uniqueFingerprint[item].PixelColor == currentPixel)
                                {
                                    matchCount++;

                                    //draw on output to demonstrate finding
                                    if (testMode)
                                        screenShotUpdate.DrawRectangle(Pens.Blue, columnPixel + uniqueFingerprint[item].XLocation, rowPixel + uniqueFingerprint[item].YLocation, 5, 5);
                                }
                                else
                                {
                                    //mismatch in the pixel series, not a series of matching coordinate
                                    //?add threshold %?
                                    imageFound = false;

                                    //draw on output to demonstrate finding
                                    if (testMode)
                                        screenShotUpdate.DrawRectangle(Pens.OrangeRed, columnPixel + uniqueFingerprint[item].XLocation, rowPixel + uniqueFingerprint[item].YLocation, 5, 5);
                                }
                            }

                            double matchAccuracy = (double)matchCount / (double)uniqueFingerprint.Count();

                            if (matchAccuracy >= vSelectedAccuracy)
                            {
                                imageFound = true;

                                var leftX = columnPixel;
                                var middleX = columnPixel + userImageMaxWidth / 2;
                                var rightX = columnPixel + userImageMaxWidth;
                                var topY = rowPixel;
                                var middleY = rowPixel + userImageMaxHeight / 2;
                                var bottomY = rowPixel + userImageMaxHeight;

                                if (testMode)
                                {
                                    //draw on output to demonstrate finding
                                    var Rectangle = new Rectangle(leftX, topY, userImageMaxWidth, userImageMaxHeight);
                                    Brush brush = new SolidBrush(Color.ForestGreen);
                                    screenShotUpdate.FillRectangle(brush, Rectangle);
                                }

                                switch (v_ImageAction)
                                {
                                    case "Click Image":
                                        string clickType = (from rw in v_ImageActionParameterTable.AsEnumerable()
                                                             where rw.Field<string>("Parameter Name") == "Click Type"
                                                             select rw.Field<string>("Parameter Value")).FirstOrDefault();
                                        string clickPosition = (from rw in v_ImageActionParameterTable.AsEnumerable()
                                                            where rw.Field<string>("Parameter Name") == "Click Position"
                                                            select rw.Field<string>("Parameter Value")).FirstOrDefault();
                                        int xAdjust = Convert.ToInt32((from rw in v_ImageActionParameterTable.AsEnumerable()
                                                                           where rw.Field<string>("Parameter Name") == "X Adjustment"
                                                                           select rw.Field<string>("Parameter Value")).FirstOrDefault().ConvertToUserVariable(engine));

                                        int yAdjust = Convert.ToInt32((from rw in v_ImageActionParameterTable.AsEnumerable()
                                                                           where rw.Field<string>("Parameter Name") == "Y Adjustment"
                                                                           select rw.Field<string>("Parameter Value")).FirstOrDefault().ConvertToUserVariable(engine));

                                        int clickPositionX = 0;
                                        int clickPositionY = 0;
                                        switch (clickPosition)
                                        {
                                            case "Center":
                                                clickPositionX = middleX;
                                                clickPositionY = middleY;
                                                break;
                                            case "Top Left":
                                                clickPositionX = leftX;
                                                clickPositionY = topY;
                                                break;
                                            case "Top Middle":
                                                clickPositionX = middleX;
                                                clickPositionY = topY;
                                                break;
                                            case "Top Right":
                                                clickPositionX = rightX;
                                                clickPositionY = topY;
                                                break;
                                            case "Bottom Left":
                                                clickPositionX = leftX;
                                                clickPositionY = bottomY;
                                                break;
                                            case "Bottom Middle":
                                                clickPositionX = middleX;
                                                clickPositionY = bottomY;
                                                break;
                                            case "Bottom Right":
                                                clickPositionX = rightX;
                                                clickPositionY = bottomY;
                                                break;
                                            case "Middle Left":
                                                clickPositionX = leftX;
                                                clickPositionY = middleX;
                                                break;
                                            case "Middle Right":
                                                clickPositionX = rightX;
                                                clickPositionY = middleY;
                                                break;
                                        }
                                        //move mouse to position
                                        var mouseMove = new SendMouseMoveCommand
                                        {
                                            v_XMousePosition = (clickPositionX + xAdjust).ToString(),
                                            v_YMousePosition = (clickPositionY + yAdjust).ToString(),
                                            v_MouseClick = clickType
                                        };

                                        mouseMove.RunCommand(sender);
                                        break;
                                    case "Set Text":
                                        string textToSet = (from rw in v_ImageActionParameterTable.AsEnumerable()
                                                            where rw.Field<string>("Parameter Name") == "Text To Set"
                                                            select rw.Field<string>("Parameter Value")).FirstOrDefault().ConvertToUserVariable(engine);

                                        string encryptedData = (from rw in v_ImageActionParameterTable.AsEnumerable()
                                                                where rw.Field<string>("Parameter Name") == "Encrypted Text"
                                                                select rw.Field<string>("Parameter Value")).FirstOrDefault();

                                        if (encryptedData == "Encrypted")
                                            textToSet = EncryptionServices.DecryptString(textToSet, "TASKT");

                                        //move mouse to position and set text
                                        var setTextMouseMove = new SendMouseMoveCommand
                                        {
                                            v_XMousePosition = (middleX).ToString(),
                                            v_YMousePosition = (middleY).ToString(),
                                            v_MouseClick = "Left Click"
                                        };
                                        setTextMouseMove.RunCommand(sender);

                                        SendKeys.SendWait(textToSet);
                                        break;
                                    case "Set Secure Text":
                                        var secureString = (from rw in v_ImageActionParameterTable.AsEnumerable()
                                                            where rw.Field<string>("Parameter Name") == "Secure String Variable"
                                                            select rw.Field<string>("Parameter Value")).FirstOrDefault();

                                        var secureStrVariable = VariableMethods.LookupVariable(engine, secureString);

                                        if (secureStrVariable.VariableValue is SecureString)
                                            secureString = ((SecureString)secureStrVariable.VariableValue).ConvertSecureStringToString();
                                        else
                                            throw new ArgumentException("Provided Argument is not a 'Secure String'");

                                        //move mouse to position and set text
                                        var setSecureTextMouseMove = new SendMouseMoveCommand
                                        {
                                            v_XMousePosition = (middleX).ToString(),
                                            v_YMousePosition = (middleY).ToString(),
                                            v_MouseClick = "Left Click"
                                        };
                                        setSecureTextMouseMove.RunCommand(sender);

                                        SendKeys.SendWait(secureString);
                                        break;

                                    case "Check If Image Exists":
                                        var outputVariable = (from rw in v_ImageActionParameterTable.AsEnumerable()
                                                               where rw.Field<string>("Parameter Name") == "Output Variable Name"
                                                               select rw.Field<string>("Parameter Value")).FirstOrDefault();

                                        //remove brackets from variable
                                        outputVariable = outputVariable.Replace("{", "").Replace("}", "");
                                        imageFound.ToString().StoreInUserVariable(engine, outputVariable);
                                        break;
                                    case "Wait For Image To Exist":
                                    default:
                                        break;
                                       
                                }
                            }
                        }

                        if (imageFound)
                            break;
                    }
                    catch (Exception)
                    {
                        //continue
                    }
                }

                if (imageFound)
                    break;
            }

            if (testMode)
            {
                frmImageCapture captureOutput = new frmImageCapture();
                captureOutput.pbTaggedImage.Image = sampleOut;
                captureOutput.pbSearchResult.Image = desktopOutput;
                captureOutput.Show();
                captureOutput.TopMost = true;
                //captureOutput.WindowState = FormWindowState.Maximized;
            }

            userImage.Dispose();
            desktopImage.Dispose();
            screenShotUpdate.Dispose();
            CommandControls.ShowAllForms();

            if (!imageFound)
            {
                if (v_ImageAction == "Check If Image Exists")
                {
                    var outputVariable = (from rw in v_ImageActionParameterTable.AsEnumerable()
                                          where rw.Field<string>("Parameter Name") == "Output Variable Name"
                                          select rw.Field<string>("Parameter Value")).FirstOrDefault();

                    //remove brackets from variable
                    outputVariable = outputVariable.Replace("{", "").Replace("}", "");
                    imageFound.ToString().StoreInUserVariable(engine, outputVariable);
                }
                else                       
                    throw new Exception("Specified image was not found in window!");
            }
        }
        public override List<Control> Render(IfrmCommandEditor editor)
        {
            base.Render(editor);

            UIPictureBox imageCapture = new UIPictureBox();
            imageCapture.Width = 200;
            imageCapture.Height = 200;
            imageCapture.DataBindings.Add("EncodedImage", this, "v_ImageCapture", false, DataSourceUpdateMode.OnPropertyChanged);

            RenderedControls.Add(CommandControls.CreateDefaultLabelFor("v_ImageCapture", this));
            RenderedControls.AddRange(CommandControls.CreateUIHelpersFor("v_ImageCapture", this, new Control[] { imageCapture }, editor));
            RenderedControls.Add(imageCapture);

            _imageActionDropdown = (ComboBox)CommandControls.CreateDropdownFor("v_ImageAction", this);
            RenderedControls.Add(CommandControls.CreateDefaultLabelFor("v_ImageAction", this));
            RenderedControls.AddRange(CommandControls.CreateUIHelpersFor("v_ImageAction", this, new Control[] { _imageActionDropdown }, editor));
            _imageActionDropdown.SelectionChangeCommitted += ImageAction_SelectionChangeCommitted;
            RenderedControls.Add(_imageActionDropdown);

            _imageParameterControls = new List<Control>();
            _imageParameterControls.Add(CommandControls.CreateDefaultLabelFor("v_ImageActionParameterTable", this));
            _imageParameterControls.AddRange(CommandControls.CreateUIHelpersFor("v_ImageActionParameterTable", this, new Control[] { _imageGridViewHelper }, editor));
            _imageParameterControls.Add(_imageGridViewHelper);
            RenderedControls.AddRange(_imageParameterControls);

            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_TimeoutSeconds", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_MatchAccuracy", this, editor));

            return RenderedControls;
        }

        public override string GetDisplayValue()
        {
            return base.GetDisplayValue() + $" [{v_ImageAction} on Screen - Accuracy '{v_MatchAccuracy}']";
        }

        public void ImageAction_SelectionChangeCommitted(object sender, EventArgs e)
        {
            SurfaceAutomationCommand cmd = this;
            DataTable actionParameters = cmd.v_ImageActionParameterTable;

            if (sender != null)
                actionParameters.Rows.Clear();

            switch (_imageActionDropdown.SelectedItem)
            {
                 case "Click Image":
                    foreach (var ctrl in _imageParameterControls)
                        ctrl.Show();

                    DataGridViewComboBoxCell mouseClickTypeBox = new DataGridViewComboBoxCell();
                    mouseClickTypeBox.Items.Add("Left Click");
                    mouseClickTypeBox.Items.Add("Middle Click");
                    mouseClickTypeBox.Items.Add("Right Click");
                    mouseClickTypeBox.Items.Add("Left Down");
                    mouseClickTypeBox.Items.Add("Middle Down");
                    mouseClickTypeBox.Items.Add("Right Down");
                    mouseClickTypeBox.Items.Add("Left Up");
                    mouseClickTypeBox.Items.Add("Middle Up");
                    mouseClickTypeBox.Items.Add("Right Up");
                    mouseClickTypeBox.Items.Add("Double Left Click");

                    DataGridViewComboBoxCell mouseClickPositionBox = new DataGridViewComboBoxCell();
                    mouseClickPositionBox.Items.Add("Center");
                    mouseClickPositionBox.Items.Add("Top Left");
                    mouseClickPositionBox.Items.Add("Top Middle");
                    mouseClickPositionBox.Items.Add("Top Right");
                    mouseClickPositionBox.Items.Add("Bottom Left");
                    mouseClickPositionBox.Items.Add("Bottom Middle");
                    mouseClickPositionBox.Items.Add("Bottom Right");
                    mouseClickPositionBox.Items.Add("Middle Left");
                    mouseClickPositionBox.Items.Add("Middle Right");

                    if (sender != null)
                    {
                        actionParameters.Rows.Add("Click Type", "");
                        actionParameters.Rows.Add("Click Position", "");
                        actionParameters.Rows.Add("X Adjustment", 0);
                        actionParameters.Rows.Add("Y Adjustment", 0);
                    }

                    if (sender != null)
                    {
                        _imageGridViewHelper.Rows[0].Cells[1].Value = "Left Click";
                        _imageGridViewHelper.Rows[1].Cells[1].Value = "Center";
                    }

                    _imageGridViewHelper.Rows[0].Cells[1] = mouseClickTypeBox;
                    _imageGridViewHelper.Rows[1].Cells[1] = mouseClickPositionBox;
                    break;

                case "Set Text":
                    foreach (var ctrl in _imageParameterControls)
                        ctrl.Show();

                    if (sender != null)
                    {
                        actionParameters.Rows.Add("Text To Set");
                        actionParameters.Rows.Add("Encrypted Text");
                        actionParameters.Rows.Add("Optional - Click to Encrypt 'Text To Set'");

                        DataGridViewComboBoxCell encryptedBox = new DataGridViewComboBoxCell();
                        encryptedBox.Items.Add("Not Encrypted");
                        encryptedBox.Items.Add("Encrypted");
                        _imageGridViewHelper.Rows[1].Cells[1] = encryptedBox;
                        _imageGridViewHelper.Rows[1].Cells[1].Value = "Not Encrypted";

                        var buttonCell = new DataGridViewButtonCell();
                        _imageGridViewHelper.Rows[2].Cells[1] = buttonCell;
                        _imageGridViewHelper.Rows[2].Cells[1].Value = "Encrypt Text";
                        _imageGridViewHelper.CellContentClick += ImageGridViewHelper_CellContentClick;
                    }
                    break;

                case "Set Secure Text":
                    foreach (var ctrl in _imageParameterControls)
                        ctrl.Show();

                    if (sender != null)
                        actionParameters.Rows.Add("Secure String Variable");
                    
                    break;

                case "Check If Image Exists":
                    foreach (var ctrl in _imageParameterControls)
                        ctrl.Show();

                    if (sender != null)
                        actionParameters.Rows.Add("Output Variable Name", "");
                    break;

                 case "Wait For Image To Exist":
                    foreach (var ctrl in _imageParameterControls)
                        ctrl.Hide();
                    break;

                default:
                    break;
            }
        }

        private void ImageGridViewHelper_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var targetCell = _imageGridViewHelper.Rows[e.RowIndex].Cells[e.ColumnIndex];

            if (targetCell is DataGridViewButtonCell && targetCell.Value.ToString() == "Encrypt Text")
            {
                var targetElement = _imageGridViewHelper.Rows[0].Cells[1];

                if (string.IsNullOrEmpty(targetElement.Value.ToString()))
                    return;

                var warning = MessageBox.Show($"Warning! Text should only be encrypted one time and is not reversible in the builder. " +
                                               "Would you like to proceed and convert '{targetElement.Value.ToString()}' to an encrypted value?",
                                               "Encryption Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (warning == DialogResult.Yes)
                {
                    targetElement.Value = EncryptionServices.EncryptString(targetElement.Value.ToString(), "TASKT");
                    _imageGridViewHelper.Rows[2].Cells[1].Value = "Encrypted";
                }
            }
        }
    }
}