using System.Data;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Windows;
using NPOI.XSSF.UserModel;

namespace CheckUtils
{
    /// <summary>
    /// 使用NPOI技术读取和导出excel，无需office组件
    /// </summary>
    class OperationExcel
    {
        /// <summary>  
        ///  生成excel表格 ，DataTable从0行0列开始，excelWorksheet从0行0列开始
        ///  参数说明  dt：DataTable， excelFilename：文件名，offset:列中的偏移量
        /// </summary>  
        public static bool ExportToExcel(DataTable dt, string filePathAndName, int offset)
        {
            try
            {
                //说明：HSSFWorkbook 用于创建  .xls
                //创建EXCEL中的Workbook  
                IWorkbook workbook = new HSSFWorkbook();
                //创建样式
                ICellStyle style = workbook.CreateCellStyle();
                IFont font = workbook.CreateFont();
                font.FontHeightInPoints = 12;
                font.FontName = "Times New Roman";
                font.Boldweight = (short)FontBoldWeight.None;
                style.SetFont(font);
                //创建Workbook中的Sheet  
                ISheet sheet = workbook.CreateSheet("sheet1");
                //创建Sheet中的Row  
                IRow rowHead = sheet.CreateRow(0);
                //设置表头信息
                for (int i = 0; i < dt.Columns.Count - offset; i++)
                {
                    rowHead.CreateCell(i).SetCellValue(dt.Columns[i + offset].ColumnName);
                    //设置表头样式
                    rowHead.GetCell(i).CellStyle = style;
                }
                //将数据导入到工作表的单元格
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    IRow rowBody = sheet.CreateRow(i + 1);
                    //一行中的数据赋值
                    for (int j = 0; j < dt.Columns.Count - offset; j++)
                    {
                        //Excel单元格第一个从索引0开始
                        rowBody.CreateCell(j).SetCellValue(dt.Rows[i][j + offset].ToString());
                        //设置样式
                        rowBody.GetCell(j).CellStyle = style;
                    }
                }
                //保存excel表
                FileStream file = new FileStream(filePathAndName, FileMode.Create);
                workbook.Write(file);
                workbook.Close();
                file.Close();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("操作失败，请重试；可能原因:" + ex.Message, "消息", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// 将excel导入到datatable
        /// </summary>
        /// <param name="filePath">excel路径</param>
        /// <param name="isColumnName">第一行是否是列名</param>
        /// <returns>返回datatable</returns>
        public static DataTable ExcelToDataTable(string filePath, bool isColumnName)
        {
            DataTable dataTable = null;
            FileStream fs = null;
            DataColumn column = null;
            DataRow dataRow = null;
            IWorkbook workbook = null;
            ISheet sheet = null;
            IRow row = null;
            ICell cell = null;
            int startRow = 0;
            try
            {
                using (fs = File.OpenRead(filePath))//using代码段执行完后自动释放内部资源
                {
                    // 2007版本
                    if (filePath.IndexOf(".xlsx") > 0)
                        workbook = new XSSFWorkbook(fs);
                    // 2003版本
                    else if (filePath.IndexOf(".xls") > 0)
                        workbook = new HSSFWorkbook(fs);

                    if (workbook != null)
                    {
                        //读取第一个sheet，当然也可以循环读取每个sheet
                        sheet = workbook.GetSheetAt(0);
                        dataTable = new DataTable();
                        if (sheet != null)
                        {
                            int rowCount = sheet.LastRowNum;//总行数
                            if (rowCount > 0)
                            {
                                IRow firstRow = sheet.GetRow(0);//第一行
                                int cellCount = firstRow.LastCellNum;//列数

                                //构建datatable的列
                                //如果第一行是标题栏（字段名）
                                if (isColumnName)
                                {
                                    startRow = 1;//如果第一行是列名，则从第二行开始读取数据区，便于第142行起填充行
                                    for (int i = firstRow.FirstCellNum; i < cellCount; i++)
                                    {
                                        cell = firstRow.GetCell(i);
                                        if (cell != null)
                                        {
                                            if (cell.StringCellValue != null)
                                            {
                                                column = new DataColumn(cell.StringCellValue);//标题栏（字段名）
                                                dataTable.Columns.Add(column);//增加一列
                                            }
                                        }
                                    }
                                }
                                //如果第一行没有标题栏
                                else
                                {
                                    for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                                    {
                                        column = new DataColumn("column" + (i + 1));
                                        dataTable.Columns.Add(column);
                                    }
                                }

                                //填充行
                                for (int i = startRow; i <= rowCount; ++i)
                                {
                                    //获取表格中一行
                                    row = sheet.GetRow(i);
                                    if (row == null) continue;
                                    //创建具有相同架构的行
                                    dataRow = dataTable.NewRow();
                                    //依次将表格中每个单元格中的数据赋值给dataRow中的每个单元格
                                    for (int j = row.FirstCellNum; j < cellCount; ++j)
                                    {
                                        cell = row.GetCell(j);
                                        if (cell == null)
                                        {
                                            dataRow[j] = "";
                                        }
                                        else
                                        {
                                            //CellType(Unknown = -1,Numeric = 0,String = 1,Formula = 2,Blank = 3,Boolean = 4,Error = 5,)
                                            switch (cell.CellType)
                                            {
                                                case CellType.Blank:
                                                    dataRow[j] = "";
                                                    break;
                                                case CellType.Numeric:
                                                    short format = cell.CellStyle.DataFormat;
                                                    //对时间格式（2015.12.5、2015/12/5、2015-12-5等）的处理
                                                    if (format == 14 || format == 31 || format == 57 || format == 58)
                                                        dataRow[j] = cell.DateCellValue;
                                                    else
                                                        dataRow[j] = cell.NumericCellValue;
                                                    break;
                                                case CellType.String:
                                                    dataRow[j] = cell.StringCellValue;
                                                    break;
                                            }
                                        }
                                    }
                                    //将赋值好的一行加入到dataTable
                                    dataTable.Rows.Add(dataRow);
                                }
                            }
                        }
                    }
                }
                return dataTable;
            }
            catch (Exception e)
            {
                if (fs != null)
                {
                    fs.Close();
                }
                MessageBox.Show("Error:" + e.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
    }
}
