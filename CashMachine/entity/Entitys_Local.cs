/// <summary>
/// 各个实体类
/// </summary>
namespace CashMachine.entity_local
{
    /// <summary>
    /// 发票抬头
    /// </summary>
    public class HeaderOfInvoice
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public string Line { get; set; }
        public int Flag { get; set; }//标志位

        public override string ToString()
        {
            return "[ id=" +Id+", number="+Number+ ", line=" + Line + ", flag=" + Flag + "]";
        }
    }

    /// <summary>
    /// 2 收银员
    /// </summary>
    public class Cashier
    {
        public string Id { get; set; }
        public int Number { get; set; }//编号
        public string Name { get; set; }//名称
        public string Code { get; set; }//操作员代码，3位，注意 1）Code禁止重复 2）CODE不能设置为000
        public string Password { get; set; }//密码
        public int Flag { get; set; }//标志位
        //public string UserId { get; set; }//用户Id

        public override string ToString()
        {
            return "[ Id=" + Id + ", Number=" + Number + ", Name=" + Name + ", Code=" + Code + ", Password=" + Password + ", Flag=" + Flag + " ]";
        }
    }

    /// <summary>
    /// 3 税率
    /// </summary>
    public class Tax
    {
        public string Id { get; set; }//Id
        public string Number { get; set; }//编号
        public string Invoice_Code { get; set; }
        public string Invoice_Name { get; set; }
        public string Code { get; set; }//代码
        public string Name { get; set; }//名称
        public string Rate { get; set; }//税率值
        public string Exempt_Flag { get; set; }//编号
        public string CRC32 { get; set; }//编号

        public override string ToString()
        {
            return "[ Id=" + Id + ", Number=" + Number + ", Invoice_Code=" + Invoice_Code + ", Invoice_Name=" + Invoice_Name + ", Code=" + Code + ", Name=" + Name + ", Rate=" + Rate + ", Exempt_Flag=" + Exempt_Flag + ", CRC32=" + CRC32 + " ]";
        }
    }

    /// <summary>
    /// 4 部类关联
    /// </summary>
    public class Department
    {
        public string Id { get; set; }//Id
        public string Dept_No { get; set; }//编号
        public string PLU_No { get; set; }//
        public string UserId { get; set; }//用户Id

        public override string ToString()
        {
            return "[ Id=" + Id + ", Dept_No=" + Dept_No + ", PLU_No=" + PLU_No + ", UserId=" + UserId + " ]";
        }
    }

    /// <summary>
    /// 5 折扣加和折扣减
    /// </summary>
    public class Discount
    {
        public string Id { get; set; }//Id
        public string Increase_Rate { get; set; }//
        public string Decrease_Rate { get; set; }//
        public string Report_Password { get; set; }//
        public string Program_Password { get; set; }//
        public string Service_Password { get; set; }//
        public string Receipt_Upper_Price_Limit { get; set; }//上限
        public string UserId { get; set; }//用户Id

        public override string ToString()
        {
            return "[ Id=" + Id + ", Increase_Rate=" + Increase_Rate + ", Decrease_Rate=" + Decrease_Rate + ", Report_Password=" + Report_Password + ", Program_Password=" + Program_Password + ", Service_Password=" + Service_Password + ", Receipt_Upper_Price_Limit=" + Receipt_Upper_Price_Limit + " ]";
        }
    }

    /// <summary>
    /// 6 外币
    /// </summary>
    public class ForeignCurrency
    {
        public string Id { get; set; }//Id
        public string Number { get; set; }//编号
        public string Name { get; set; }//编号
        public string Code { get; set; }//
        public string Abbreviation { get; set; }//
        public string Symbol { get; set; }//
        public string Symbol_Direction { get; set; }//
        public string Thousand_Separator { get; set; }//
        public string Cent_Separator { get; set; }//
        public string Decimal_Places { get; set; }//
        public double Exchange_Rate { get; set; }//
        public int Flag { get; set; }//标志位
        public int Current { get; set; }//当前外币
        public string UserId { get; set; }//用户Id

        public override string ToString()
        {
            return "[ Id=" + Id + ", Number=" + Number + ", Code=" + Code + ", Abbreviation=" + Abbreviation + ", Symbol=" + Symbol 
                + ", Symbol_Direction=" + Symbol_Direction + ", Thousand_Separator=" + Thousand_Separator + ", Cent_Separator=" + Cent_Separator 
                + ", Decimal_Places=" + Decimal_Places + ", Exchange_Rate=" + Exchange_Rate + ", Flag=" + Flag + ", UserId=" + UserId + " ]";
        }
    }

    /// <summary>
    /// 7 单品 id,Number,Name,Barcode,Price,Tax_Index,Stock_Control,Stock_Amount
    /// </summary>
    public class Plu
    {
        public string Id { get; set; }//Id
        public string Number { get; set; }//编号
        public string Name { get; set; }
        public string Barcode { get; set; }
        public double Price { get; set; }
        public double RRP { get; set; }
        public string Tax_Index { get; set; }
        public int Stock_Control { get; set; }//库存管理控制
        public double Stock_Amount { get; set; }//库存
        public string Tax_Code { get; set; }
        public string Currency { get; set; }//当前外币缩写
        public string Used { get; set; }
        public override string ToString()
        {
            return "[ Id=" + Id + ", Number=" + Number + ", Name=" + Name + ", Barcode=" + Barcode + ", Price=" + Price + ", Tax_Index=" + RRP + ", RRP=" + Tax_Index
                +  ", Stock_Control=" + Stock_Control + ", Stock_Amount=" + Stock_Amount +  " ]";
        }
    }

    //id,Date_Time,Receipt_No,Z_Number,Name,Unit_Price,Quantity,Item_Sum,Dept_Index,VAT_Index,VAT_Rate,PLU_No,Sub_Group,Discount_Flag
    /// <summary>
    /// 8 销售数据
    /// </summary>
    public class SalesItem
    {
        public string Id { get; set; }//Id
        public long Date_Time { get; set; }//日期
        public int Receipt_No { get; set; }//
        public int Z_Number { get; set; }//
        public string Name { get; set; }//
        public double Unit_Price { get; set; }//
        public double Quantity { get; set; }//库存管理控制
        public double Item_Sum { get; set; }//
        public double Dept_Index { get; set; }//
        public int VAT_Index { get; set; }//
        public double VAT_Rate { get; set; }//
        public string PLU_No { get; set; }//
        public int Sub_Group { get; set; }//
        public int Discount_Flag { get; set; }//

        public override string ToString()
        {
            return "[ Id=" + Id + ", Date_Time=" + Date_Time + ", Receipt_No=" + Receipt_No + ", Z_Number=" + Z_Number + ", Name=" + Name + ", Unit_Price=" + Unit_Price
                + ", Quantity=" + Quantity + ", Item_Sum=" + Item_Sum + ", Dept_Index=" + Dept_Index + ", VAT_Index=" + VAT_Index
                + ", VAT_Rate=" + VAT_Rate + ", PLU_No=" + PLU_No + ", Sub_Group=" + Sub_Group + ", Discount_Flag=" + Discount_Flag + " ]";
        }
    }

    /// <summary>
    /// 9 客户 id,Number,Name,BPN,VAT,Address,Tel,Bank_Account_No,Remark,Reserved
    /// </summary>
    public class Buyer
    {
        public string Id { get; set; }//Id
        public string Number { get; set; }//编号
        public string Name { get; set; }//
        public string BPN { get; set; }//
        public string VAT { get; set; }//
        public string Address { get; set; }//
        public string Tel { get; set; }//
        public string Bank_Account_No { get; set; }//
        public string Remark { get; set; }//
        public string Reserved { get; set; }//
        public override string ToString()
        {
            return "[ Id=" + Id + ", Number=" + Number + ", Name=" + Name + ", BPN=" + BPN + ", VAT=" + VAT + ", Address=" + Address
                + ", Tel=" + Tel + ", Bank_Account_No=" + Bank_Account_No + ", Remark=" + Remark + ", Reserved=" + Reserved + " ]";
        }
    }
}
