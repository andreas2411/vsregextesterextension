using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RegexTester.UserControls;
using Forms = System.Windows.Forms;
using RegexTester;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web.UI.Design;
using EnvDTE;
using RegexTester.Parsing.VB;
using RegexTester.Parsing.CSharp;
using Microsoft.VisualStudio.Shell;
using RegexTester.Parsing;

namespace AndreasAndersen.Regular_Expression_Tester_Extension
{
    /// <summary>
    /// Interaction logic for RegexTesterControl.xaml
    /// </summary>
    public partial class RegexTesterControl : UserControl
    {
        private SyntaxHighlightRichTextBox regularExpressionRichTextBox;
        private Forms.RichTextBox richTextBoxMatches;
        private RegexTesterModel model;
        private System.Threading.Thread updateThread;
        private Stopwatch monitorWatch = new Stopwatch();
        private System.Threading.Thread monitorThread;
        private IRegexFindResults regexFindResults;
        private TextDocument doc;
        private EditPoint startPoint;
        private EditPoint endPoint;

        public RegexTesterControl()
        {
            InitializeComponent();

            regularExpressionRichTextBox = new SyntaxHighlightRichTextBox();
            regularExpressionRichTextBox.BorderStyle = Forms.BorderStyle.None;
            regularExpressionRichTextBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.syntaxHighlighterHost.Child = regularExpressionRichTextBox;

            richTextBoxMatches = new Forms.RichTextBox();
            richTextBoxMatches.ReadOnly = true;
            Color richTextBoxBackgroundColor = (textBoxReplaceResult.Background as SolidColorBrush).Color;
            richTextBoxMatches.BackColor = System.Drawing.Color.FromArgb(richTextBoxBackgroundColor.R, richTextBoxBackgroundColor.G, richTextBoxBackgroundColor.B);
            richTextBoxMatches.BorderStyle = Forms.BorderStyle.None;
            richTextBoxMatches.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxMatchesHost.Child = richTextBoxMatches;

            CheckBox[] checkBoxes = new[] { checkBoxCompiled, checkBoxCultureInvariant, checkBoxECMAScript, checkBoxExplicitCapture,
                checkBoxIgnoreCase, checkBoxIgnorePatternWhitespace, checkBoxMultiline, checkBoxRightToLeft, checkBoxSingleline };

            foreach (CheckBox checkBox in checkBoxes)
            {
                checkBox.Checked += regexParameters_Changed;
                checkBox.Unchecked += regexParameters_Changed;
            }

            regularExpressionRichTextBox.TextChanged += regexParameters_Changed;
            textBoxItemToMatch.TextChanged += regexParameters_Changed;
            textBoxReplacePattern.TextChanged += regexParameters_Changed;
            buttonEvaluate.Click += regexParameters_Changed;
            ToggleAutomaticEvaluation(true);

            model = new RegexTesterModel();
            RefreshLoadedRegexsView();

            updateThread = new System.Threading.Thread(UpdateFormThreadStart);
            updateThread.Start();
            monitorThread = new System.Threading.Thread(MonitorUpdateThreadStart);
            monitorThread.Start();

            Loaded += new RoutedEventHandler(ControlLoaded);
        }

        public EnvDTE.DTE DTE { get; set; }

        public ToolWindowPane ToolWindow { get; set; }

        private void ControlLoaded(object sender, EventArgs e)
        {
            lock (testFormUpdateIndexLock)
            {
                TestFormUpdateIndex++;
            }
        }

        public void Reset()
        {
            if (DTE != null && DTE.ActiveDocument != null)
            {
                this.doc = this.DTE.ActiveDocument.Object("") as TextDocument;
                RegexFinder regexFinder;
                if (this.DTE.ActiveDocument.ProjectItem == null || this.DTE.ActiveDocument.ProjectItem.ContainingProject == null ||
                    this.DTE.ActiveDocument.ProjectItem.ContainingProject.CodeModel == null)
                {
                    regexFinder = this.DTE.ActiveDocument.Name.EndsWith(".cs") ? new RegexFinder(new CSharpRegexFormatProvider()) : new RegexFinder(new VBRegexFormatProvider());
                }
                else
                {
                    regexFinder = this.DTE.ActiveDocument.ProjectItem.ContainingProject.CodeModel.Language == "{B5E9BD34-6D3E-4B5D-925E-8A43B79820B4}" ?
                        new RegexFinder(new CSharpRegexFormatProvider()) :
                        new RegexFinder(new VBRegexFormatProvider());
                }
                startPoint = doc.Selection.TopPoint.CreateEditPoint();
                endPoint = doc.Selection.BottomPoint.CreateEditPoint();
                regexFindResults = regexFinder.FindRegex(doc.Selection.Text);
                if (regexFindResults.FoundMatch)
                {
                    regexFindResults.ConvertToDisplay();
                    this.RegularExpression = regexFindResults.Regex;
                    if (regexFindResults.ToMatch != null)
                    {
                        this.ToMatch = regexFindResults.ToMatch;
                    }
                    if (regexFindResults.ReplacePattern != null)
                    {
                        this.ReplacePattern = regexFindResults.ReplacePattern;
                    }
                    this.Options = regexFindResults.RegexOptions;
                }
                else
                {
                    this.RegularExpression = "";
                    this.ToMatch = "";
                    this.ReplacePattern = "";
                    this.ReplaceResult = "";
                }
            }
            else
            {
                doc = null;
                startPoint = null;
                endPoint = null;
                regexFindResults = null;
            }
        }

        private bool IsDisposed { get; set; }

        private void RegexTesterForm_Disposed(object sender, EventArgs e)
        {
            IsDisposed = true;
        }

        private List<RegexInfo> CurrentRegexs { get; set; }

        private void RefreshLoadedRegexsView()
        {
            CurrentRegexs = model.GetRegularExpressions().ToList();
            if (CurrentRegexInfo != null)
            {
                CurrentRegexInfo = (from re in CurrentRegexs where re.Id == CurrentRegexInfo.Id select re).Single();
            }
            this.dataGridViewRegexs.ItemsSource = CurrentRegexs;
            if (CurrentRegexInfo != null)
            {
                this.dataGridViewTestCases.ItemsSource = CurrentRegexInfo.TestCaseInfos;
                if (CurrentTestCaseInfo != null)
                {
                    CurrentTestCaseInfo = (from tc in CurrentRegexInfo.TestCaseInfos where tc.Id == CurrentTestCaseInfo.Id select tc).Single();
                }
            }
            else
            {
                this.dataGridViewTestCases.ItemsSource = null;
            }
        }

        private object testFormUpdateIndexLock = new object();
        private int TestFormUpdateIndex { get; set; }

        private void UpdateFormThreadStart()
        {
            int maxIndex = -1;
            while (!IsDisposed)
            {
                bool updated = false;
                lock (testFormUpdateIndexLock)
                {
                    if (TestFormUpdateIndex > maxIndex)
                    {
                        maxIndex = TestFormUpdateIndex;
                        updated = true;
                    }
                }
                if (updated)
                {
                    monitorWatch.Start();
                    UpdateTestForm();
                    monitorWatch.Reset();
                }
                else
                {
                    System.Threading.Thread.Sleep(100);
                }
            }
        }

        private void MonitorUpdateThreadStart()
        {
            while (!IsDisposed)
            {
                if (labelWarningEvaluationTime.IsVisible != monitorWatch.Elapsed.Seconds > 2)
                {
                    CallGUIFromOtherThread(() => { labelWarningEvaluationTime.Visibility = labelWarningEvaluationTime.IsVisible ? Visibility.Hidden : Visibility.Visible; });
                }
                System.Threading.Thread.Sleep(100);
            }
        }

        private void buttonResetEvaluation_Click(object sender, EventArgs e)
        {
            updateThread.Abort();
            updateThread.Join();
            ToMatch = "";
            updateThread = new System.Threading.Thread(UpdateFormThreadStart);
            updateThread.Start();
        }

        /// <summary>
        /// Regular expression evaluated by form.
        /// </summary>
        public string RegularExpression
        {
            get { return regularExpressionRichTextBox.Text; }
            set { regularExpressionRichTextBox.Text = value; }
        }

        public string ToMatch
        {
            get { return textBoxItemToMatch.Text; }
            set { textBoxItemToMatch.Text = value; }
        }

        public string ReplacePattern
        {
            get { return textBoxReplacePattern.Text; }
            set { textBoxReplacePattern.Text = value; }
        }

        public string ReplaceResult
        {
            get { return textBoxReplaceResult.Text; }
            set { textBoxReplaceResult.Text = value; }
        }

        public RegexOptions Options
        {
            set
            {
                checkBoxCompiled.IsChecked = (value & RegexOptions.Compiled) == RegexOptions.Compiled;
                checkBoxCultureInvariant.IsChecked = (value & RegexOptions.CultureInvariant) == RegexOptions.CultureInvariant;
                checkBoxECMAScript.IsChecked = (value & RegexOptions.ECMAScript) == RegexOptions.ECMAScript;
                checkBoxExplicitCapture.IsChecked = (value & RegexOptions.ExplicitCapture) == RegexOptions.ExplicitCapture;
                checkBoxIgnoreCase.IsChecked = (value & RegexOptions.IgnoreCase) == RegexOptions.IgnoreCase;
                checkBoxIgnorePatternWhitespace.IsChecked = (value & RegexOptions.IgnorePatternWhitespace) == RegexOptions.IgnorePatternWhitespace;
                checkBoxMultiline.IsChecked = (value & RegexOptions.Multiline) == RegexOptions.Multiline;
                checkBoxRightToLeft.IsChecked = (value & RegexOptions.RightToLeft) == RegexOptions.RightToLeft;
                checkBoxSingleline.IsChecked = (value & RegexOptions.Singleline) == RegexOptions.Singleline;
            }
            get
            {
                RegexOptions regexOptions = RegexOptions.None;
                regexOptions |= checkBoxCompiled.IsChecked.Value ? RegexOptions.Compiled : RegexOptions.None;
                regexOptions |= checkBoxCultureInvariant.IsChecked.Value ? RegexOptions.CultureInvariant : RegexOptions.None;
                regexOptions |= checkBoxECMAScript.IsChecked.Value ? RegexOptions.ECMAScript : RegexOptions.None;
                regexOptions |= checkBoxExplicitCapture.IsChecked.Value ? RegexOptions.ExplicitCapture : RegexOptions.None;
                regexOptions |= checkBoxIgnoreCase.IsChecked.Value ? RegexOptions.IgnoreCase : RegexOptions.None;
                regexOptions |= checkBoxIgnorePatternWhitespace.IsChecked.Value ? RegexOptions.IgnorePatternWhitespace : RegexOptions.None;
                regexOptions |= checkBoxMultiline.IsChecked.Value ? RegexOptions.Multiline : RegexOptions.None;
                regexOptions |= checkBoxRightToLeft.IsChecked.Value ? RegexOptions.RightToLeft : RegexOptions.None;
                regexOptions |= checkBoxSingleline.IsChecked.Value ? RegexOptions.Singleline : RegexOptions.None;
                return regexOptions;
            }
        }

        private RegexInfo CurrentRegexInfo { get; set; }
        private TestCaseInfo CurrentTestCaseInfo { get; set; }

        private delegate void InvokeDelegate();
        private void CallGUIFromOtherThread(Action a)
        {
            this.Dispatcher.Invoke(new InvokeDelegate(a));
        }

        private ParsedRegex CurrentParsedRegex { get; set; }

        private string CurrentItemToMatchText { get; set; }

        private string CurrentReplacePatternText { get; set; }

        private bool BuildResultTreeAutomatically { get; set; }

        private bool BuildResultTreeManually { get; set; }

        private void UpdateTestForm()
        {
            string regularExpressionText = (string)null;
            RegexOptions options = RegexOptions.None;
            this.CallGUIFromOtherThread((Action)(() =>
            {
                regularExpressionText = this.regularExpressionRichTextBox.Text;
                options = this.Options;
                this.CurrentReplacePatternText = this.textBoxReplacePattern.Text;
                this.CurrentItemToMatchText = this.textBoxItemToMatch.Text;
            }));
            this.CurrentParsedRegex = this.model.GetRegularExpression(regularExpressionText, options);
            bool isValid = this.CurrentParsedRegex.Regex != null;
            this.CallGUIFromOtherThread((Action)(() => this.syntaxHighlighterBorder.BorderBrush = (System.Windows.Media.Brush)new SolidColorBrush(isValid ? Colors.Green : Colors.Red)));
            if (isValid)
            {
                if (this.BuildResultTreeAutomatically || this.BuildResultTreeManually)
                    this.BuildResultTree();
                else
                    this.CallGUIFromOtherThread((Action)(() => this.messageLabel.Text = ""));
            }
            else
                this.CallGUIFromOtherThread((Action)(() => this.messageLabel.Text = this.CurrentParsedRegex.ParseError));
        }

        private void BuildResultTree()
        {
            if (this.CurrentParsedRegex != null && this.CurrentParsedRegex.Regex != null && this.CurrentItemToMatchText != null)
            {
                RegexResultTreeNode resultTree = this.model.BuildMatchTree(this.CurrentParsedRegex.Regex, this.CurrentItemToMatchText);
                this.CallGUIFromOtherThread((Action)(() =>
                {
                    this.richTextBoxMatches.Clear();
                    resultTree.Accept((IRegexResultTreeNodeVisitor)new RegexResultTreeToRichTextVisitor(this.richTextBoxMatches));
                }));
                string replaceValue = this.CurrentParsedRegex.Regex.Replace(this.CurrentItemToMatchText, this.CurrentReplacePatternText);
                this.CallGUIFromOtherThread((Action)(() =>
                {
                    this.textBoxReplaceResult.Text = replaceValue;
                    this.messageLabel.Text = "";
                }));
            }
            this.BuildResultTreeManually = false;
        }

        private void SetCurrentRegexInfo(RegexInfo current, bool clearMatch, bool switchTab)
        {
            this.CurrentRegexInfo = current;
            if (this.CurrentRegexInfo != null)
            {
                this.RegularExpression = current.Regex ?? "";
                this.ReplacePattern = current.ReplacePattern ?? "";
                this.Options = current.Options;
                buttonSaveTest.IsEnabled = true;
                buttonSaveTestAs.IsEnabled = true;
                tabItemTestCases.Visibility = System.Windows.Visibility.Visible;
                this.dataGridViewTestCases.ItemsSource = CurrentRegexInfo.TestCaseInfos;
                if (this.CurrentRegexInfo.TestCaseInfos != null && this.CurrentRegexInfo.TestCaseInfos.Count > 0)
                {
                    SetCurrentTestCaseInfo(this.CurrentRegexInfo.TestCaseInfos[0], false);
                }
                else if (clearMatch)
                {
                    SetCurrentTestCaseInfo(null, false);
                }
            }
            else
            {
                this.RegularExpression = "";
                this.ReplacePattern = "";
                this.Options = RegexOptions.None;
                buttonSaveTest.IsEnabled = false;
                buttonSaveTestAs.IsEnabled = false;
                tabItemTestCases.Visibility = System.Windows.Visibility.Hidden;
                this.dataGridViewTestCases.ItemsSource = null;
                if (clearMatch)
                {
                    SetCurrentTestCaseInfo(null, false);
                }
            }
            SetToolWindowTitle();
            if (switchTab)
            {
                tabControlRegex.SelectedIndex = 0;
            }
        }

        private void SetToolWindowTitle()
        {
            if (CurrentRegexInfo != null && CurrentRegexInfo.Name != null)
            {
                ToolWindow.Caption = string.Format(@"Test Regular Expressions - {0}", CurrentRegexInfo.Name);
            }
            else
            {
                ToolWindow.Caption = "Test Regular Expressions";
            }
        }

        private void SetCurrentTestCaseInfo(TestCaseInfo current, bool switchTab)
        {
            this.CurrentTestCaseInfo = current;
            if (current != null)
            {
                this.textBoxItemToMatch.Text = current.TextToMatch;
            }
            else
            {
                this.textBoxItemToMatch.Text = "";
            }
            if (switchTab)
            {
                tabControlRegex.SelectedIndex = 0;
            }
        }

        private void regexParameters_Changed(object sender, EventArgs e)
        {
            lock (testFormUpdateIndexLock)
            {
                TestFormUpdateIndex++;
            }
        }

        private void buttonInsert_Click(object sender, EventArgs e)
        {
            if (regexFindResults != null && DTE != null && DTE.ActiveDocument != null)
            {
                regexFindResults.Regex = RegularExpression;
                if (!string.IsNullOrEmpty(ReplacePattern))
                {
                    regexFindResults.ReplacePattern = ReplacePattern;
                }
                if (!string.IsNullOrEmpty(ToMatch) && regexFindResults.ToMatch != null)
                {
                    regexFindResults.ToMatch = ToMatch;
                }
                regexFindResults.RegexOptions = Options;
                regexFindResults.ConvertToEdit();
                if (regexFindResults.FoundMatch)
                {
                    doc.Selection.MoveToPoint(startPoint, false);
                    doc.Selection.MoveToPoint(endPoint, true);
                    doc.Selection.Delete();
                    doc.Selection.Insert(regexFindResults.GetRegexString());
                    endPoint = doc.Selection.ActivePoint.CreateEditPoint();
                    doc.Selection.MoveToPoint(startPoint, false);
                    doc.Selection.MoveToPoint(endPoint, true);
                }
                else
                {
                    doc.Selection.ActivePoint.CreateEditPoint().Insert(regexFindResults.GetRegexString());
                }
            }
        }

        private void buttonNew_Click(object sender, EventArgs e)
        {
            SetCurrentRegexInfo(null, true, false);
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (CurrentRegexInfo != null && CurrentRegexInfo.Name != null)
            {
                PopulateAndSaveRegularExpression(CurrentRegexInfo);
                SetCurrentRegexInfo(CurrentRegexInfo, false, false);
                RefreshLoadedRegexsView();
            }
            else
            {
                buttonSaveAs_Click(null, null);
            }
        }

        private void buttonSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveWindow saveWindow = new SaveWindow("Save Regular Expression");
            saveWindow.ShowDialog();
            if (saveWindow.DialogResult.GetValueOrDefault())
            {
                RegexInfo regexInfo = new RegexInfo
                {
                    Id = Guid.NewGuid(),
                    Name = saveWindow.Name
                };
                CurrentRegexs.Add(regexInfo);
                PopulateAndSaveRegularExpression(regexInfo);
                SetCurrentRegexInfo(regexInfo, false, false);
            }
            RefreshLoadedRegexsView();
        }

        private void PopulateAndSaveRegularExpression(RegexInfo toSave)
        {
            toSave.Options = this.Options;
            toSave.Regex = this.RegularExpression;
            toSave.ReplacePattern = this.ReplacePattern;
            model.SaveRegexs(CurrentRegexs);
        }

        private void buttonSaveTest_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentTestCaseInfo != null)
            {
                CurrentTestCaseInfo.TextToMatch = textBoxItemToMatch.Text;
                model.SaveRegexs(CurrentRegexs);
                RefreshLoadedRegexsView();
            }
            else
            {
                buttonSaveTestAs_Click(null, null);
            }
        }

        private void buttonSaveTestAs_Click(object sender, RoutedEventArgs e)
        {
            SaveWindow saveWindow = new SaveWindow("Save Test");
            saveWindow.ShowDialog();
            if (saveWindow.DialogResult.GetValueOrDefault())
            {
                TestCaseInfo testCaseInfo = new TestCaseInfo
                {
                    Id = Guid.NewGuid(),
                    Name = saveWindow.Name,
                    TextToMatch = textBoxItemToMatch.Text
                };
                CurrentRegexInfo.TestCaseInfos.Add(testCaseInfo);
                CurrentTestCaseInfo = testCaseInfo;
                model.SaveRegexs(CurrentRegexs);
                RefreshLoadedRegexsView();
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            List<RegexInfo> newList = new List<RegexInfo>();
            IDictionary<Guid, Guid> toDelete = new Dictionary<Guid, Guid>();
            foreach (var item in dataGridViewRegexs.SelectedItems)
            {
                RegexInfo regexInfo = item as RegexInfo;
                toDelete.Add(regexInfo.Id, regexInfo.Id);
                if (CurrentRegexInfo != null && regexInfo.Id == CurrentRegexInfo.Id)
                {
                    SetCurrentRegexInfo(null, true, false);
                }
            }
            for (int i = 0; i < CurrentRegexs.Count; i++)
            {
                if (!toDelete.ContainsKey(CurrentRegexs[i].Id))
                {
                    newList.Add(CurrentRegexs[i]);
                }
            }
            model.SaveRegexs(newList);
            RefreshLoadedRegexsView();
        }

        private void buttonRenameRegularExpression_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridViewRegexs.SelectedItems.Count == 1)
            {
                RegexInfo regexInfo = (RegexInfo)dataGridViewRegexs.SelectedItems[0];
                SaveWindow renameWindow = new SaveWindow("Rename Regular Expression") { Name = regexInfo.Name };
                renameWindow.ShowDialog();
                if (renameWindow.DialogResult.GetValueOrDefault())
                {
                    regexInfo.Name = renameWindow.Name;
                    model.SaveRegexs(CurrentRegexs);
                    RefreshLoadedRegexsView();
                    SetToolWindowTitle();
                }
            }
        }

        private void dataGridViewRegexs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            RegexInfo toLoad = CurrentRegexs[dataGridViewRegexs.SelectedIndex];
            SetCurrentRegexInfo(toLoad, true, true);
            e.Handled = true;
        }

        private void buttonDeleteTest_Click(object sender, EventArgs e)
        {
            HashSet<Guid> toDelete = new HashSet<Guid>();
            foreach (TestCaseInfo testCaseInfo in dataGridViewTestCases.SelectedItems)
            {
                toDelete.Add(testCaseInfo.Id);
                if (CurrentTestCaseInfo != null && testCaseInfo.Id == CurrentTestCaseInfo.Id)
                {
                    SetCurrentTestCaseInfo(null, false);
                }
            }
            CurrentRegexInfo.TestCaseInfos = CurrentRegexInfo.TestCaseInfos.Where(tci => !toDelete.Contains(tci.Id)).ToList();
            model.SaveRegexs(CurrentRegexs);
            RefreshLoadedRegexsView();
        }

        private void buttonRenameTestCase_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridViewTestCases.SelectedItems.Count == 1)
            {
                TestCaseInfo testCaseInfo = (TestCaseInfo)dataGridViewTestCases.SelectedItems[0];
                SaveWindow renameWindow = new SaveWindow("Rename Test Case") { Name = testCaseInfo.Name };
                renameWindow.ShowDialog();
                if (renameWindow.DialogResult.GetValueOrDefault())
                {
                    testCaseInfo.Name = renameWindow.Name;
                    model.SaveRegexs(CurrentRegexs);
                    RefreshLoadedRegexsView();
                }
            }
        }

        private void dataGridViewTestCases_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TestCaseInfo toLoad = CurrentRegexInfo.TestCaseInfos[dataGridViewTestCases.SelectedIndex];
            SetCurrentTestCaseInfo(toLoad, true);
            e.Handled = true;
        }

        private void editItemToMatchButton_Click(object sender, RoutedEventArgs e)
        {
            TextEditor editor = new TextEditor() { Text = ToMatch, IsEditable = true };
            editor.ShowDialog();
            if (editor.DialogResult.HasValue && editor.DialogResult.Value)
            {
                ToMatch = editor.Text;
            }
        }

        private void viewReplaceResultButton_Click(object sender, RoutedEventArgs e)
        {
            TextEditor editor = new TextEditor() { Text = ReplaceResult, IsEditable = false };
            editor.ShowDialog();
        }

        private void viewMatchesButton_Click(object sender, RoutedEventArgs e)
        {
            RichTextEditor editor = new RichTextEditor() { Text = richTextBoxMatches.Rtf, IsEditable = false };
            editor.ShowDialog();
        }

        private void buttonEvaluate_Click(object sender, RoutedEventArgs e)
        {
            this.BuildResultTreeManually = true;
        }

        private void checkboxEvaluateAutomatically_Click(object sender, RoutedEventArgs e)
        {
            ToggleAutomaticEvaluation(this.checkboxEvaluateAutomatically.IsChecked.Value);
            this.buttonEvaluate.Visibility = this.checkboxEvaluateAutomatically.IsChecked.Value ? Visibility.Collapsed : Visibility.Visible;
        }

        private void ToggleAutomaticEvaluation(bool evaluateAutomatically)
        {
            this.BuildResultTreeAutomatically = evaluateAutomatically;
            this.buttonEvaluate.Visibility = evaluateAutomatically ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    internal class RegexResultTreeToRichTextVisitor : IRegexResultTreeNodeVisitor
    {
        private Forms.RichTextBox richTextBox;
        private Stack<System.Drawing.Color> colorStack;

        internal RegexResultTreeToRichTextVisitor(Forms.RichTextBox richTextBox)
        {
            this.richTextBox = richTextBox;
            this.colorStack = new Stack<System.Drawing.Color>();
        }

        public void Visit(RootNode node, bool beforeChildren)
        {

        }

        public void Visit(MatchNode node, bool beforeChildren)
        {
            System.Drawing.Font currentFont = richTextBox.SelectionFont;
            System.Drawing.Font boldFont = new System.Drawing.Font(currentFont, System.Drawing.FontStyle.Bold);
            richTextBox.SelectionFont = boldFont;
            if (beforeChildren)
            {
                colorStack.Push(richTextBox.SelectionColor);
                richTextBox.SelectionColor = System.Drawing.Color.Green;
            }
            richTextBox.AppendText(beforeChildren ? "[" : "]");
            if (!beforeChildren)
            {
                richTextBox.SelectionColor = colorStack.Pop();
            }
            richTextBox.SelectionFont = currentFont;
        }

        public void Visit(GroupNode node, bool beforeChildren)
        {
            System.Drawing.Font currentFont = richTextBox.SelectionFont;
            System.Drawing.Font boldFont = new System.Drawing.Font(currentFont, System.Drawing.FontStyle.Bold);
            richTextBox.SelectionFont = boldFont;
            if (beforeChildren)
            {
                colorStack.Push(richTextBox.SelectionColor);
                richTextBox.SelectionColor = System.Drawing.Color.Red;
            }
            richTextBox.AppendText(beforeChildren ? "(<" + node.GroupName + ">" : ")");
            if (!beforeChildren)
            {
                richTextBox.SelectionColor = colorStack.Pop();
            }
            richTextBox.SelectionFont = currentFont;
        }

        public void Visit(LiteralNode node)
        {
            richTextBox.AppendText(node.Literal);
        }
    }
}