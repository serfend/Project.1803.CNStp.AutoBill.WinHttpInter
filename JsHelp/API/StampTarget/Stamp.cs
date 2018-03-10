using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JsHelp.API.StampTarget
{
	class Stamp
	{
		private double price;
		private string maxBuyNum;
		private BillBuild goods;
		public Stamp(string stampId)
		{
			goods = new BillBuild(stampId);
		}
	}
	public class BillBuild
	{
		private buyGoodsNowBean goods;
		private string buy_type;
		private string goodsTicketAttr;

		public BillBuild(string id)
		{
			var attrId = "";
			var goodsNum = "";
			this.goods = new buyGoodsNowBean(id,attrId,goodsNum);
		}

		public BillBuild(buyGoodsNowBean goods, string buy_type, string goodsTicketAttr)
		{
			this.Goods = goods;
			this.Buy_type = buy_type;
			this.GoodsTicketAttr = goodsTicketAttr;
		}

		public buyGoodsNowBean Goods { get => goods; set => goods = value; }
		public string Buy_type { get => buy_type; set => buy_type = value; }
		public string GoodsTicketAttr { get => goodsTicketAttr; set => goodsTicketAttr = value; }
	}
	public class buyGoodsNowBean
	{
		private string goods_id;
		private string goods_attr_id;
		private string goods_num;

		public buyGoodsNowBean(string goods_id, string goods_attr_id, string goods_num)
		{
			this.Goods_id = goods_id;
			this.Goods_attr_id = goods_attr_id;
			this.Goods_num = goods_num;
		}

		public string Goods_id { get => goods_id; set => goods_id = value; }
		public string Goods_attr_id { get => goods_attr_id; set => goods_attr_id = value; }
		public string Goods_num { get => goods_num; set => goods_num = value; }
	}
}
