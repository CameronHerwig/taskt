using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
    public class ImageRecognitionCommand : ScriptCommand
    {
        [XmlAttribute]
        [PropertyDescription("Capture Search Image")]       
        [InputSpecification("Use the tool to capture an image.")]
        [SampleUsage("")]
        [Remarks("The image will be used as the image to be found on screen.")]
        [PropertyUIHelper(UIAdditionalHelperType.ShowImageRecogitionHelper)]
        public string v_ImageCapture { get; set; }

        [XmlAttribute]
        [PropertyDescription("Offset X Coordinate")]
        [InputSpecification("Specify if an offset is required.")]
        [SampleUsage("0 || 100 || {vXCoordinate}")]
        [Remarks("This will move the mouse X pixels to the right of the location of the image. This input is optional.")]
        [PropertyUIHelper(UIAdditionalHelperType.ShowVariableHelper)]
        public string v_XOffsetAdjustment { get; set; }

        [XmlAttribute]
        [PropertyDescription("Offset Y Coordinate")]
        [InputSpecification("Specify if an offset is required.")]
        [SampleUsage("0 || 100 || {vYCoordinate}")]
        [Remarks("This will move the mouse X pixels down from the top of the location of the image. This input is optional.")]
        [PropertyUIHelper(UIAdditionalHelperType.ShowVariableHelper)]
        public string v_YOffsetAdjustment { get; set; }

        [XmlAttribute]
        [PropertyDescription("Mouse Click Type")]
        [PropertyUISelectionOption("None")]
        [PropertyUISelectionOption("Left Click")]
        [PropertyUISelectionOption("Middle Click")]
        [PropertyUISelectionOption("Right Click")]
        [PropertyUISelectionOption("Left Down")]
        [PropertyUISelectionOption("Middle Down")]
        [PropertyUISelectionOption("Right Down")]
        [PropertyUISelectionOption("Left Up")]
        [PropertyUISelectionOption("Middle Up")]
        [PropertyUISelectionOption("Right Up")]
        [PropertyUISelectionOption("Double Left Click")]
        [InputSpecification("Indicate the type of click required")]
        [SampleUsage("")]
        [Remarks("You can simulate custom clicking by using multiple mouse click commands in succession, adding a **Pause Command** in between where required.")]
        public string v_MouseClick { get; set; }

        [XmlAttribute]
        [PropertyDescription("Mouse Click Position")]
        [PropertyUISelectionOption("Center")]
        [PropertyUISelectionOption("Top Left")]
        [PropertyUISelectionOption("Top Middle")]
        [PropertyUISelectionOption("Top Right")]
        [PropertyUISelectionOption("Bottom Left")]
        [PropertyUISelectionOption("Bottom Middle")]
        [PropertyUISelectionOption("Bottom Right")]
        [PropertyUISelectionOption("Middle Left")]
        [PropertyUISelectionOption("Middle Right")]
        [InputSpecification("Indicate the position of the click relative to the selected image.")]
        [SampleUsage("")]
        [Remarks("")]
        public string v_MousePosition { get; set; }

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

        public ImageRecognitionCommand()
        {
            CommandName = "ImageRecognitionCommand";
            SelectionName = "Image Recognition";
            CommandEnabled = true;
            CustomRendering = true;

            v_XOffsetAdjustment = "0";
            v_YOffsetAdjustment = "0";
            v_TimeoutSeconds = "0";
            v_MouseClick = "Left Click";
            v_MousePosition = "Center";
            v_MatchAccuracy = "0.8";
        }

        public override void RunCommand(object sender)
        {
            var engine = (AutomationEngineInstance)sender;
            bool testMode = TestMode;

            int vXOffset = int.Parse(v_XOffsetAdjustment.ConvertToUserVariable(engine));
            int vYOffset = int.Parse(v_YOffsetAdjustment.ConvertToUserVariable(engine));
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

                                int clickPositionX = 0;
                                int clickPositionY = 0;

                                switch (v_MousePosition)
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
                                    v_XMousePosition = (clickPositionX + vXOffset).ToString(),
                                    v_YMousePosition = (clickPositionY + vYOffset).ToString(),
                                    v_MouseClick = v_MouseClick
                                };

                                mouseMove.RunCommand(sender);
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
                throw new Exception("Specified image was not found in window!");
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

            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_XOffsetAdjustment", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_YOffsetAdjustment", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultDropdownGroupFor("v_MouseClick", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultDropdownGroupFor("v_MousePosition", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_TimeoutSeconds", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_MatchAccuracy", this, editor));

            return RenderedControls;
        }

        public override string GetDisplayValue()
        {
            if (v_MouseClick == "None")
                return base.GetDisplayValue() + $" [Find Image on Screen - Accuracy '{v_MatchAccuracy}']";
            else
                return base.GetDisplayValue() + $" [Find and {v_MouseClick} in {v_MousePosition} of Image on Screen - Accuracy '{v_MatchAccuracy}']";
        }
    }
}