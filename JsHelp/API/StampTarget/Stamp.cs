using DotNet4.Utilities.UtilCode;
using DotNet4.Utilities.UtilHttp;
using DotNet4.Utilities.UtilReg;
using JsHelp.API.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsHelp.API.StampTarget
{
	public class Stamp
	{
		private BillBuild bill;
		private User.User user;
		public Stamp(User.User user, string stampId, string buyNum, Action<Stamp> CallBack = null)
		{
			this.User = user;
			Bill = new BillBuild(this,stampId, buyNum, CallBack);
		}
		public void ShowOut()
		{
			Logger.SysLog("邮票基础数据："+this.ToString());
		}
		public override string ToString()
		{
			var cstr = new StringBuilder();
			cstr.Append("id:").Append(bill.Goods.Goods_id).Append("\n")
				.Append("Goods_attr_id:").Append(bill.Goods.Goods_attr_id).Append("\n")
				.Append("GoodsTicketAttr:").Append(bill.GoodsTicketAttr).Append("\n")
				.Append("userSelectNum:").Append(bill.Goods.Goods_num).Append("\n")
				.Append("limit:").Append(bill.BuyLimitNum);
			return cstr.ToString();
		}
		public BillBuild Bill { get => bill; set => bill = value; }
		internal User.User User { get => user; set => user = value; }
		public Bill TargetBill { get => targetBill; set => targetBill = value; }

		private Bill targetBill;
		public bool SynBillInfo(User.User user)
		{
			//TODO 同步生成的订单的信息
			if (!Bill.Init) { user.LogInfo("邮票数据未加载完成时不可同步订单信息"); return false; }
			targetBill = new StampTarget.Bill(this);
			return true;
		}
	}
	public class Bill
	{
		Stamp parent;
		private bool init;
		public Bill(Stamp parent)
		{
			this.parent = parent;
			parent.User.UserRequest.JSONGetUserInfoByUserId((userInfo) => {
				

			},parent);



		}

		public bool Init { get => init; set => init = value; }
	}
	public class BillBuild
	{
		private Stamp parent;

		private string price;
		private string buyLimitNum;

		private buyGoodsNowBean goods;
		private string buy_type;
		private string goodsTicketAttr;
		private bool init=false;
		public string GetBillPostData
		{
			get
			{
				return string.Format("buyGoodsNowBean.goods_id={0}&buy_type={1}&buyGoodsNowBean.goods_attr_id={2}&buyGoodsNowBean.goods_num={3}&goodsTicketAttr={4}", goods.Goods_id, buy_type, goods.Goods_attr_id,goods.Goods_num ,goodsTicketAttr);
			}
		}
		public BillBuild(Stamp parent, string id, string goodsNum, Action<Stamp> CallBack = null)
		{
			GetStampInfo(parent,id, goodsNum,CallBack);
		}
		private void GetStampInfo(Stamp parent, string id,string goodsNum, Action<Stamp> CallBack = null)
		{
			this.parent = parent;
			var buildStamp = new Task(()=> {
				parent.User.UserRequest.Http.ClearItem();
				goods = new buyGoodsNowBean() {Goods_id=id};
				var response = parent.User.UserRequest.Http.GetHtml(GoodsPageUrl).document.response;
				var info = response.DataString();
				goods.Goods_attr_id = HttpUtil.GetElement(info, "\"attrId\":\"","\"");
				BuyLimitNum = HttpUtil.GetElement(info, "\"buyLimit\":\"","\"");
				buy_type = HttpUtil.GetElement(info, "salesTypes\":\"", "\"");
				goodsTicketAttr = HttpUtil.GetElement(info, "\"id\":", ",");
				price = HttpUtil.GetElement(info, "\"price\":", "}");
				goods.Goods_num = goodsNum;
				Init = true;
				CallBack?.Invoke(parent);
			});
			buildStamp.Start();
		}
		public string GoodsPageUrl { get =>string.Format("http://jiyou.retail.11185.cn/retail/ticketDetail_{0}.html",goods.Goods_id ); }
		public BillBuild(buyGoodsNowBean goods, string buy_type, string goodsTicketAttr)
		{
			this.Goods = goods;
			this.Buy_type = buy_type;
			this.GoodsTicketAttr = goodsTicketAttr;
			Init = true;
		}

		public buyGoodsNowBean Goods { get => goods; set => goods = value; }
		public string Buy_type { get => buy_type; set => buy_type = value; }
		public string GoodsTicketAttr { get => goodsTicketAttr; set => goodsTicketAttr = value; }
		public bool Init { get => init; set => init = value; }
		public string BuyLimitNum { get => buyLimitNum; set => buyLimitNum = value; }
	}
	public class buyGoodsNowBean
	{
		private string goods_id;
		private string goods_attr_id;
		private string goods_num;
		public string Goods_id { get => goods_id; set => goods_id = value; }
		public string Goods_attr_id { get => goods_attr_id; set => goods_attr_id = value; }
		public string Goods_num { get => goods_num; set => goods_num = value; }
	}
}
