using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExcelDna.Integration;
using ExcelDna.Integration.CustomUI;
using Microsoft.Office.Interop.Excel;
using TygerbergNeonatalAddin.Properties;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.Diagnostics;
using ExcelDna.Integration.Extensibility;

namespace TygerbergNeonatalAddin
{
    public class Addin : IExcelAddIn
    {
        // consider showing a messagebox before e.g. changing the filter type to prevent the user from accidentally deleting data

        // todo license

        // todo add more analysis (e.g. list criteria, etc.)

        // todo about dialog

        // todo execute mso can fail (throw exception) if you are in formula bar


        // todo check that you can work with these headers (do the throwexceptionifthingdoesnotuniquelyidentify)

        // TODO you need to update the checks in preprocessingmodel since this transformation assumes that the date column header has properly specified dates.
        
            // catch ExpectedDateColumn thing...

        private static PreprocessingForm filtersConfigForm;
        private static AnalysisForm analysisConfigForm;

        private static Application app => (Application)(ExcelDnaUtil.Application);
        private static Model model;
        private static string ConfigPath => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "//NeonatalExtensionsConfig.xml";
        private static DataContractSerializer serializer;
        private static bool deactivated = false;
        
        private static string FormatTransformationError(ITransformation transformation, string specificErrorInformation)
        {
            string transformationIdentification;

            if (transformation is Filter)
            {
                Filter filter = (Filter)transformation;
                transformationIdentification = "the filter '" + filter.Name + " (" + filter.FilterType.Description() + ")'";
            }
            else
            {
                transformationIdentification = "the transformation";
            }

            return string.Format(@"An error occurred while trying to run {0}:

{1}

Consequently, no filters have been applied and the workbook has been left as-is.", transformationIdentification, specificErrorInformation);
        }

        private static bool SerializeConfigToPathWithErrorHandling(string path, Model model, bool primaryConfig)
        {
            System.Windows.Forms.DialogResult? result = null;
            try
            {
                var file = File.Create(path);
                serializer.WriteObject(file, model);
                file.Close();
            }
            catch (IOException e)
            {
                result = System.Windows.Forms.MessageBox.Show(
                    "The file at " + path + " could not be opened. Details of the error are shown below.\n\n" + e.ToString()
                    + (primaryConfig ? "\n\nPlease be aware that any changes you have made to the configuration, including imports, will not be persisted after Excel is closed." : ""),
                    "",
                    System.Windows.Forms.MessageBoxButtons.RetryCancel);
            }
            catch (SerializationException e)
            {
                result = System.Windows.Forms.MessageBox.Show(
                    "The file at " + path + " could be opened but an error occurred while saving the configuration to it. Details of the error are shown below.\n\n" + e.ToString()
                    + (primaryConfig ? "\n\nPlease be aware that any changes you have made to the configuration, including imports, will not be persisted after Excel is closed." : ""),
                    "",
                    System.Windows.Forms.MessageBoxButtons.RetryCancel);
            }

            if (result == null) return true;
            else if (result == System.Windows.Forms.DialogResult.Retry)
            {
                return SerializeConfigToPathWithErrorHandling(path, model, primaryConfig);
            }
            else
            {
                return false;
            }
        }

        private static bool DeserializeConfigToPathWithErrorHandling(string path, out Model ret, bool primaryConfig)
        {
            ret = null;

            System.Windows.Forms.DialogResult? result = null;
            try
            {
                var file = new FileStream(path, FileMode.Open);
                ret = (Model)(serializer.ReadObject(file));
                file.Close();
            }
            catch (IOException e)
            {
                result = System.Windows.Forms.MessageBox.Show(
                    "The file at " + path + " could not be opened. Details of the error are shown below\n\n" + e.ToString()
                    + (primaryConfig ? "\n\nTo generate a new, empty configuration, delete the file at this location and restart Excel." : ""),
                    "",
                    System.Windows.Forms.MessageBoxButtons.RetryCancel);
            }
            catch (SerializationException e)
            {
                result = System.Windows.Forms.MessageBox.Show(
                    "The file at " + path + " could be opened but an error occurred while parsing it. Make sure the configuration is well-formed. Details of the error are shown below.\n\n" + e.ToString()
                    + (primaryConfig ? "\n\nTo generate a new, empty configuration, delete the file at this location and restart Excel." : ""),
                    "",
                    System.Windows.Forms.MessageBoxButtons.RetryCancel);
            }

            if (result == null)
            {
                return true;
            }
            else if (result == System.Windows.Forms.DialogResult.Retry)
            {
                return DeserializeConfigToPathWithErrorHandling(path, out ret, primaryConfig);
            }
            else
            {
                return false;
            }
        }

        public void AutoOpen()
        {
            System.Windows.Forms.Application.EnableVisualStyles();

            serializer = new DataContractSerializer(typeof(Model));

            if (File.Exists(ConfigPath))
            {
                if (!DeserializeConfigToPathWithErrorHandling(ConfigPath, out model, primaryConfig: true))
                {
                    deactivated = true;
                }
            }
            else
            {
                model = new Model();
            }
        }

        public void AutoClose()
        {

        }

        [ComVisible(true)]
        public class RibbonController : ExcelRibbon
        {
            public override string GetCustomUI(string RibbonID)
            {
                if (deactivated) return null;

                return @"
      <customUI xmlns='http://schemas.microsoft.com/office/2006/01/customui'>
      <ribbon>
        <tabs>
          <tab id='tab1' label='Tygerberg Neonatal'>
            <group id='groupFiltering' label='Filtering'>
              <dynamicMenu id='dynamicMenuRunFilter' label='Run filter' getContent='GetDynamicMenuRunFilterContent' invalidateContentOnDrop='true' size='normal'/>
              <button id='buttonConfigureFilters' label='Configure filters' onAction='OnConfigureFiltersClicked'/>
            </group >
            <group id='groupAnalysis' label='Analysis'>
              <button id='buttonFindClusters' label='Find clusters' onAction='OnFindClustersClicked'/>
              <button id='buttonConfigureAnalysis' label='Configure analysis' onAction='OnConfigureAnalysisClicked'/>
            </group >
            <group id='groupAddin' label='Add-in'>
              <button id='buttonAbout' label='About' onAction='OnAboutClicked'/>
              <button id='buttonImport' label='Import configuration' onAction='OnImportClicked'/>
              <button id='buttonExport' label='Export configuration' onAction='OnExportClicked'/>
            </group >
          </tab>
        </tabs>
      </ribbon>
    </customUI>";
            }
            
            public override void OnBeginShutdown(ref Array custom)
            {
                SerializeConfigToPathWithErrorHandling(ConfigPath, model, primaryConfig: true);

                base.OnBeginShutdown(ref custom);
            }

            private void Handle(Exception e)
            {
                System.Windows.Forms.ThreadExceptionDialog dlg = new System.Windows.Forms.ThreadExceptionDialog(e);
                dlg.ShowDialog();
            }

            private T InvokeWithExceptionHandling<T>(Func<T> func, bool disableExcelUpdatesDuringCall)
            {
                if (disableExcelUpdatesDuringCall)
                {
                    SetExcelUpdatesEnabled(false);
                }

                T ret;

                try
                {
                    ret = func();
                }
                catch (Exception e)
                {
                    if (disableExcelUpdatesDuringCall)
                    {
                        SetExcelUpdatesEnabled(true);
                    }

                    Handle(e);
                    throw;
                }

                if (disableExcelUpdatesDuringCall)
                {
                    SetExcelUpdatesEnabled(true);
                }

                return ret;
            }

            private void InvokeWithExceptionHandling(System.Action action, bool disableExcelUpdatesDuringCall)
            {
                if (disableExcelUpdatesDuringCall)
                {
                    SetExcelUpdatesEnabled(false);
                }

                try
                {
                    action();
                }
                catch (Exception e)
                {
                    if (disableExcelUpdatesDuringCall)
                    {
                        SetExcelUpdatesEnabled(true);
                    }

                    Handle(e);
                    throw;
                }

                if (disableExcelUpdatesDuringCall)
                {
                    SetExcelUpdatesEnabled(true);
                }
            }

            public string GetDynamicMenuRunFilterContent(IRibbonControl control)
            {
                return InvokeWithExceptionHandling(delegate ()
                {
                    string customFilters = "";
                    if (model.Filters.Count > 0)
                    {
                        for (int i = 0; i < model.Filters.Count; i++)
                        {
                            Filter filter = model.Filters[i];
                            XElement buttonElem = new XElement("button");
                            buttonElem.Add(new XAttribute("id", "buttonRunFilter" + i));
                            string label = (filter.ExcludeFromBatchProcessing ? "[E] " : "") +
                                (filter.Name == "" ? "<No name>" : filter.Name);
                            buttonElem.Add(new XAttribute("label", label));
                            buttonElem.Add(new XAttribute("onAction", "OnRunFilterClicked"));
                            customFilters += buttonElem.ToString();
                        }
                    }
                    else
                    {
                        customFilters = "<button id='buttonNoFiltersAvailable' label='&lt;No filters&gt;' enabled='false'/>";
                    }
                    return string.Format(@"
      <menu xmlns='http://schemas.microsoft.com/office/2006/01/customui'>
          <button id='buttonRunAllFilters' label='Run filters together' onAction='OnRunAllFiltersClicked' enabled='{0}'/>
          <menuSeparator id='dynamicMenuRunFilterSeparator'/>
          {1}
      </menu>", model.Filters.Count > 0 ? "true" : "false", customFilters);
                }, disableExcelUpdatesDuringCall: false);
            }

            public void OnImportClicked(IRibbonControl control)
            {
                InvokeWithExceptionHandling(delegate ()
                {
                    var dialog = new System.Windows.Forms.OpenFileDialog();
                    dialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
                    dialog.Multiselect = false;
                    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        if (System.Windows.Forms.MessageBox.Show(
                                "Importing this file will result in the current configuration being lost. Do you want to continue?\n\n(You can save the existing configuration by clicking 'Export configuration'.)",
                                "",
                                System.Windows.Forms.MessageBoxButtons.YesNo)
                                == System.Windows.Forms.DialogResult.Yes)
                        {
                            DeserializeConfigToPathWithErrorHandling(dialog.FileName, out model, primaryConfig: false);
                            SerializeConfigToPathWithErrorHandling(ConfigPath, model, primaryConfig: true);
                        }
                    }
                }, disableExcelUpdatesDuringCall: false);
            }

            public void OnExportClicked(IRibbonControl control)
            {
                InvokeWithExceptionHandling(delegate ()
                {
                    var dialog = new System.Windows.Forms.SaveFileDialog();
                    dialog.DefaultExt = "xml";
                    dialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
                    dialog.CreatePrompt = false;
                    dialog.OverwritePrompt = true;
                    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        SerializeConfigToPathWithErrorHandling(dialog.FileName, model, primaryConfig: false);
                    }
                }, disableExcelUpdatesDuringCall: false);
            }

            void ShowSimpleTransformationError(ITransformation transformation, string specificErrorExplanation)
            {
                System.Windows.Forms.MessageBox.Show(FormatTransformationError(transformation, specificErrorExplanation));
            }

            /*

            bool VerifyDatesAreProperlySpecifiedToRunTransformation(Table table, ITransformation transformation, int[] zeroBasedDateColumnsIndicies)
            {
                // TODO This is not ideal: the filters themselves should be able to report, when they are applied, that dates are not specified
                // properly. In other words, they should throw an exception and it should bubble up here as is the case with ColumnHeaderNotFoundException,
                // MultipleColumnsWithThisHeaderException and GroupingInconsistencyException. Currently, that's not possible, since Table
                // can only have strings as cell values -- and changing that means we forego compile-time type safety and we have to implement
                // type checking ourselves. Still a better solution in the long run, but I am using this hack for now.

                string dateColumnHeader = null;
                if (transformation is TimePeriodFilter) { dateColumnHeader = ((TimePeriodFilter)transformation).DateColumnHeader; }
                else if (transformation is FindClustersTransformation) { dateColumnHeader = ((FindClustersTransformation)transformation).DateColumnHeader; }
                if (dateColumnHeader != null)
                {
                    int index;
                    try
                    {
                        index = table.ColumnIndexForColumnWithHeader(dateColumnHeader);
                    }
                    catch (ArgumentDoesNotUniquelyIdentifyValueException) { return true; } // exception will be thrown later
                    catch (ArgumentOutOfRangeException) { return true; } // exception will be thrown later

                    if (!zeroBasedDateColumnsIndicies.Contains(index))
                    {
                        string message = @"This operation is configured to use the column with header '" + dateColumnHeader + @"' to find dates, but the cells therein do not properly specify dates.

To ensure that dates are resolved correctly, it is required that every cell in this column is seen as being of the Date type by Excel. Additionally, every cell must have the same value formatting.

Please make sure that the cells in the column all have the Date data type, that value formatting is uniform across the entire column, and that Excel is correctly interpreting date values in the column.";
                        ShowSimpleTransformationError(transformation, message);
                        return false;
                    }
                }

                return true;
            }

            */

            void TryRunTransformationsFromSelection(IEnumerable<ITransformation> transformations, bool inPlace)
            {
                ListObject listObject = FindListObjectInSheetFromSelection();
                if (listObject == null) return;
                Array array = ArrayFromListObject(listObject);
                Table.Column[] columns = GetColumnsFromListObject(listObject, array);
                Table t = TableFromArray(columns, array);
                
                foreach (ITransformation transformation in transformations)
                {
                    try
                    {
                        t = transformation.Apply(t);
                    }
                    catch (ColumnHeaderNotFoundException e)
                    {
                        ShowSimpleTransformationError(transformation, "The operation is configured to use the column with header '" + e.ColumnHeader + "' but the selected table does not have a column with this name.");
                        return;
                    }
                    catch (MultipleColumnsWithThisHeaderException e)
                    {
                        ShowSimpleTransformationError(transformation, "The operation is configured to use values from the column with header '" + e.ColumnHeader + "' but there is more than one column with this name in the selected table.");
                        return;
                    }
                    catch (ExpectedDateColumnException e)
                    {
                        string message = @"This operation is configured to use the column with header '" + e.ColumnHeader + @"' to find dates, but the cells therein do not properly specify dates.

To ensure that dates are resolved correctly, it is required that every cell in this column is seen as being of the Date type by Excel. Additionally, every cell must have the same value formatting.

Please make sure that the cells in the column all have the Date data type, that value formatting is uniform across the entire column, and that Excel is correctly interpreting date values in the column.";
                        ShowSimpleTransformationError(transformation, message);
                        return;
                    }
                    catch (GroupingInconsistencyException e)
                    {
                        ErrorListForm.Show(FormatTransformationError(transformation, "There are inconsistencies in the groups given for this filter. Please make sure that each member belongs to only one group."),
                            from inconsistency in e.Inconsistencies
                            select "The member " + inconsistency.Member + " belongs to " + string.Join(", ", inconsistency.GroupsMemberIsAPartOf.GetRange(0, inconsistency.GroupsMemberIsAPartOf.Count - 1))
                                + " and " + inconsistency.GroupsMemberIsAPartOf.Last() + ".");
                        return;
                    }
                    catch (CannotGroupMembersException e)
                    {
                        ErrorListForm.Show(FormatTransformationError(transformation, "Some values in the table are not part of any group, and the filter is set to show an error if members cannot be grouped."),
                            from memberDetails in e.Members select "The value '" + memberDetails.Value + "' in row " + (listObject.DataBodyRange.Row + memberDetails.Row.Index) + " does not belong to any group.");
                        return;
                    }
                }

                if (inPlace)
                {
                    WriteTableToListObject(t, listObject);
                }
                else
                {
                    SelectCellsForm form = new SelectCellsForm();
                    form.Show();
                    form.FormClosed += delegate
                    {
                        if (form.DialogResult != System.Windows.Forms.DialogResult.OK)
                        {
                            return;
                        }

                        Range selectedRange = app.Selection as Range;
                        if (selectedRange == null)
                        {
                            System.Windows.Forms.MessageBox.Show("No cells are selected.");
                        }

                        Range cell = selectedRange.Cells[1, 1];
                        TryWriteTableToNewListObject(t, cell);
                    };
                }
            }

            public void OnRunAllFiltersClicked(IRibbonControl control)
            {
                InvokeWithExceptionHandling(delegate ()
                {
                    TryRunTransformationsFromSelection(model.Filters.Where((filter) => !filter.ExcludeFromBatchProcessing), inPlace: true);
                }, disableExcelUpdatesDuringCall: true);
            }

            public void OnRunFilterClicked(IRibbonControl control)
            {
                InvokeWithExceptionHandling(delegate ()
                {
                    int index = int.Parse(control.Id.Substring("buttonRunFilter".Length));
                    Filter filter = model.Filters[index];
                    TryRunTransformationsFromSelection(new Filter[] { filter }, inPlace: true);
                }, disableExcelUpdatesDuringCall: true);
            }

            static void CreateOrFocusConfigForm<T>(ref T formRef, Func<T> createFormFunc) where T : System.Windows.Forms.Form
            {
                T form = formRef; // so we can do a null check
                if (form == null || form.IsDisposed)
                {
                    formRef = createFormFunc();
                    formRef.FormClosing += (s, e) => SerializeConfigToPathWithErrorHandling(ConfigPath, model, primaryConfig: true);
                    formRef.Show();
                }
                else
                {
                    formRef.WindowState = System.Windows.Forms.FormWindowState.Normal;
                    formRef.Focus();
                }
            }

            public void OnConfigureFiltersClicked(IRibbonControl control)
            {
                InvokeWithExceptionHandling(delegate ()
                {
                    CreateOrFocusConfigForm(ref filtersConfigForm, createFormFunc: () => new PreprocessingForm(model));
                }, disableExcelUpdatesDuringCall: false);
            }

            public void OnMonthlySummaryClicked(IRibbonControl control)
            {
                InvokeWithExceptionHandling(delegate ()
                {
                    System.Windows.Forms.MessageBox.Show("Monthly summary.");
                }, disableExcelUpdatesDuringCall: false);
            }

            public void OnConfigureAnalysisClicked(IRibbonControl control)
            {
                InvokeWithExceptionHandling(delegate ()
                {
                    CreateOrFocusConfigForm(ref analysisConfigForm, createFormFunc: () => new AnalysisForm(model));
                }, disableExcelUpdatesDuringCall: false);
            }

            public void OnFindClustersClicked(IRibbonControl control)
            {
                InvokeWithExceptionHandling(delegate ()
                {
                    TryRunTransformationsFromSelection(new ITransformation[] { model.FindClustersTransformation }, inPlace: false);
                }, disableExcelUpdatesDuringCall: true);
            }

            public void OnAboutClicked(IRibbonControl control)
            {
                InvokeWithExceptionHandling(delegate ()
                {
                    new AboutDialog().ShowDialog();
                }, disableExcelUpdatesDuringCall: false);
            }
        }

        static public bool TryWriteTableToNewListObject(Table t, Range cell)
        {
            Range range = app.Range[cell, app.Cells[cell.Row + t.RowsCount, cell.Column + t.ColumnsCount - 1]];

            Array array = ArrayFromTable(t, withHeaders: true);
            Array orig = range.Value;

            range.Value = array;

            try
            {
                ((Worksheet)(app.ActiveSheet)).ListObjects.AddEx(XlListObjectSourceType.xlSrcRange, range,
                    XlListObjectHasHeaders: XlYesNoGuess.xlYes);
            } catch (COMException)
            {
                range.Value = orig;
                System.Windows.Forms.MessageBox.Show("The data cannot be placed here. Make sure that the data will not overlap an existing table.");
                return false;
            }

            return true;
        }

        static void WriteTableToListObject(Table table, ListObject listObject)
        {
            int origRowsCount = listObject.Range.Rows.Count;
            int origColumnsCount = listObject.Range.Columns.Count;

            if (origColumnsCount != table.ColumnHeaders.Count)
            {
                throw new NotSupportedException("Number of columns for original table is not the same as the new table.");
            }

            int tRowsCountInclHeader = table.Rows.Count + 1;

            if (table.Rows.Count > 0)
            {
                listObject.Resize(listObject.Range.Resize[table.Rows.Count + 1, table.ColumnHeaders.Count]);
            }
            else
            {
                listObject.Resize(listObject.Range.Resize[2, table.ColumnHeaders.Count]);
            }

            if (tRowsCountInclHeader > origRowsCount)
            {
                throw new NotSupportedException("Writing a table that has more rows than the original is not supported.");
            }
            else if (tRowsCountInclHeader < origRowsCount)
            {
                // Clear the cells we aren't going to fill with values.
                app.Range[app.Cells[listObject.Range.Row + tRowsCountInclHeader, listObject.Range.Column],
                    app.Cells[listObject.Range.Row + origRowsCount - 1, listObject.Range.Column + origColumnsCount - 1]].Clear();
            }
            
            listObject.DataBodyRange.Value = ArrayFromTable(table, withHeaders: false);

            if (app.ActiveSheet.AutoFilter != null)
            {
                app.ActiveSheet.AutoFilter.ApplyFilter();
            }
        }

        static Array ArrayFromListObject(ListObject listObject)
        {
            if (listObject.DataBodyRange.Value is Array) return listObject.DataBodyRange.Value;
            else
            {
                Array ret = Array.CreateInstance(typeof(object), lengths: new int[] { 1, 1 }, lowerBounds: new int[] { 1, 1 });
                ret.SetValue(listObject.DataBodyRange.Value, 1, 1);
                return ret;
            }
        }
        
        static Table TableFromArray(IEnumerable<Table.Column> columns, Array ar)
        {
            Table tbl = new Table(columns);
            
            for (int row = ar.GetLowerBound(0); row <= ar.GetUpperBound(0); row++)
            {
                List<string> rowList = new List<string>();

                for (int col = ar.GetLowerBound(1); col <= ar.GetUpperBound(1); col++)
                {
                    object val = ar.GetValue(row, col);
                    if (val == null)
                    {
                        rowList.Add("");
                    }
                    else
                    {
                        rowList.Add(val.ToString());
                    }
                }

                tbl.AddRow(rowList);
            }

            return tbl;
        }

        static void SetExcelUpdatesEnabled(bool enabled)
        {
            app.ScreenUpdating = enabled;
            app.EnableEvents = enabled;
        }

        static bool IsDateColumn(ListColumn column, Array array)
        {
            if (column.DataBodyRange.NumberFormat is System.DBNull || column.DataBodyRange.NumberFormat == "") return false;
            
            for (int row = 0; row < column.DataBodyRange.Rows.Count; row++)
            {
                if (!(array.GetValue(row + 1, column.Index) is DateTime)) return false;
            }

            return true;
        }

        static Array ArrayFromTable(Table table, bool withHeaders)
        {
            Array ret = Array.CreateInstance(typeof(object), lengths: new int[] { table.Rows.Count + (withHeaders ? 1 : 0), table.ColumnsCount },
                lowerBounds: new int[] { 1, 1 });

            int rowOffset = 0;

            if (withHeaders)
            {
                for (int col = 0; col < table.ColumnsCount; col++)
                {
                    ret.SetValue(table.ColumnHeaders[col], 1, col + 1);
                }

                rowOffset++;
            }

            for (int col = 0; col < table.ColumnsCount; col++)
            {
                for (int row = 0; row < table.RowsCount; row++)
                {
                    if (table.Columns[col].typeHint == Table.TypeHint.Date)
                    {
                        ret.SetValue(DateTime.Parse(table[row][col]), row + rowOffset + 1, col + 1);
                    }
                    else
                    {
                        ret.SetValue(table[row][col], row + rowOffset + 1, col + 1);
                    }
                }
            }

            return ret;
        }

        static Table.Column[] GetColumnsFromListObject(ListObject listObject, Array array)
        {
            var ret = new Table.Column[listObject.ListColumns.Count];
            for (int i = 0; i < listObject.ListColumns.Count; i++)
            {
                string header = listObject.ListColumns[i + 1].Name;
                Table.TypeHint hint = IsDateColumn(listObject.ListColumns[i + 1], array) ? Table.TypeHint.Date : Table.TypeHint.General;
                ret[i] = new Table.Column(header, hint);
            }
            return ret;
        }

        static ListObject FindListObjectInSheetFromSelection()
        {
            var selectedRange = app.Selection as Range;
            if (selectedRange == null) { System.Windows.Forms.MessageBox.Show("No cells are selected."); return null; }

            var list = selectedRange.ListObject;
            if (list != null)
            {
                return list;
            }
            else
            {
                try
                {
                    app.CommandBars.ExecuteMso("TableInsertExcel");
                }
                catch (COMException e)
                {
                    System.Windows.Forms.MessageBox.Show("An error occurred while trying to create a table from the selection. Details are shown below.\n\n" + e.ToString());
                    return null;
                }

                selectedRange = app.Selection as Range;
                if (selectedRange == null || selectedRange.ListObject == null)
                {
                    System.Windows.Forms.MessageBox.Show("Cannot find the data set for processing.");
                    return null;
                }

                return selectedRange.ListObject;
            }
        }

    }
}
