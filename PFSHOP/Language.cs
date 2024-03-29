﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PFShop
{
    public class Language
    {
        public string CommandMain = "§r§ePixelFaramitaSHOP商店插件主菜单";
        public string CommandReload = "§r§e商店信息重载";


        public string ShopMainTitle = "商店";
        public string ShopMainContent = "来干点什么？";
        public string ShopMainSell = "出售商店";
        public string ShopMainRecycle = "回收商店";
        public string ShopMainPref= "偏好设置";

        public string CantFindConfig = "未找到配置文件";
        public string SaveDefaultConfigTo = "已将默认配置文件写入到\n{0}\n请自行修改！";
        public string CantFindLanguage = "未找到语言文件";
        public string SaveDefaultLanguageTo = "已将默认语言文件写入到\n{0}\n请自行修改！";
        public string LanguageFileLoadFailed = "语言文件加载失败，错误信息:{0}";
        public string ClosedFormWithoutAction = "§7表单已关闭，未收到操作";
        public string sellMainTitle = "点击选择你想要购买的物品";
        public string sellMainContent = "";
        public string sellPreviousList = "<==返回上级菜单";
        public string sellSubList = "§l{0}\n§o子菜单==>";
        public string sellListItem = "#{0}§l{1}\n{2}像素币/个";

        public string recycleMainTitle = "点击选择你想要回收的物品";
        public string recycleMainContent = "";
        public string recyclePreviousList = "<==返回上级菜单";
        public string recycleSubList = "§l{0}\n§o子菜单==>";
        public string recycleListItem = "#{0}§l{1}\n{2}像素币/个";

        public string InputSellDetailTitle = "输入购买数量";
        public string InputRecycleDetailTitle = "输入回收数量";
        public string InputSellDetailContentWhenUseSlider = "\n您已选择 {0}\n\n拖动滑块选择购买数量\n单价:{1}\n      §l§5X§r\n数量";
        public string InputSellDetailContentWhenUseTextbox = "\n您已选择 {0}\n\n在文本框中输入购买数量\n单价:{1}\n      §l§5X";
        public string InputSellDetailContentTextboxPlaceholder = "购买数量";
        public string InputSellDetailValueInvalid = "数值无效！";
        public string InputRecycleDetailContentWhenUseSlider = "\n您已选择 {0}\n\n拖动滑块选择回收数量\n单个收益: {1}\n         §l§5X§r\n 数  量 ";
        public string InputRecycleDetailContentWhenUseTextbox = "\n您已选择 {0}\n\n在文本框中输入回收数量\n单个收益: {1}\n         §l§5X§r";
        public string InputRecycleDetailContentTextboxPlaceholder = "回收数量";
        public string InputRecycleDetailValueInvalid = "数值无效！";

        public string confirmSellTitle = "确认购买";
        public string confirmSellContent = "购买信息:\n  名称: {0}\n  数量: {1}\n  总价: {2}\n\n点击确认即可发送购买请求";
        public string confirmSellAccept = "确认购买";
        public string confirmSellCancel = "我再想想";
        public string confirmSellCanceled = "购买已取消";

        public string confirmRecycleTitle = "确认回收";
        public string confirmRecycleContent = "回收信息:\n  名称: {0}\n  数量: {1}\n  收益: {2}\n\n点击确认即可发送回收请求";
        public string confirmRecycleAccept = "确认回收";
        public string confirmRecycleCancel = "我再想想";
        public string confirmRecycleCanceled = "回收已取消";

        public string buySuccessfullyTitle = "\n\n\n§b购买成功";
        public string buySuccessfullySubtitle = "已花费 {0} 像素币\n购买 {1} 个 {2}";
        public string buyFailedTitle = "\n\n\n§c购买失败！";
        public string buyFailedSubtitle = "购买 {1} 个 {2} 需要 {0} 像素币";

        public string recycleGetItemApiFailed = "API获取失败";
        public string recycleGetItemApiFailedDetail = "api.getPlayerItems()函数尚不支持，请使用CSR商业版运行本插件";

        public string recycleSuccessfullyTitle = "\n\n\n§a回收成功";
        public string recycleSuccessfullySubtitle = "回收了 {1} 个 {2} 获得 {0} 像素币";
        public string recycleFailedTitle = "\n\n\n§c回收失败！";
        public string recycleFailedSubtitle = "你背包里只有 {3} 个 {2}";



        #region preferenceMain
        public string preferenceMainTitle = "个人偏好设置";
        public string preferenceMainSellText = "●出售商店\n    数量输入方式";
        public string preferenceMainRecycleText = "●回收商店\n    数量输入方式";
        public string preferenceMainMaxValueTip = "    使用游标滑块输入时的最大值";
        public string preferenceMainUsingSlider = "游标滑块";
        public string preferenceMainUsingTextbox = "文本框手动输入";
        public string preferenceSaved = "个人设置保存成功";
        #endregion


        public string DefaultShopData = "{\"recycle\":[{\"type\":\"基础方块\",\"order\":\"1\",\"image\":\"textures/blocks/grass_side_carried\",\"content\":[{\"order\":\"2\",\"name\":\"泥土\",\"id\":\"dirt\",\"damage\":\"-1\",\"regex\":\"\",\"award\":1,\"image\":\"textures/blocks/dirt\"},{\"order\":\"3\",\"name\":\"木头\",\"id\":\"log\",\"damage\":\"-1\",\"regex\":\"\",\"award\":3,\"image\":\"textures/blocks/log_oak\"},{\"order\":\"4\",\"name\":\"圆石\",\"id\":\"cobblestone\",\"damage\":\"-1\",\"regex\":\"\",\"award\":2,\"image\":\"textures/blocks/cobblestone\"}]},{\"type\":\"各类矿物\",\"order\":\"30\",\"image\":false,\"content\":[{\"order\":\"31\",\"name\":\"煤炭\",\"id\":\"coal\",\"damage\":\"-1\",\"regex\":\"\",\"award\":10,\"image\":\"textures/items/coal\"},{\"order\":\"32\",\"name\":\"铁锭\",\"id\":\"iron_ingot\",\"damage\":\"-1\",\"regex\":\"\",\"award\":50,\"image\":\"textures/items/iron_ingot\"},{\"order\":\"33\",\"name\":\"金锭\",\"id\":\"gold_ingot\",\"damage\":\"-1\",\"regex\":\"\",\"award\":100,\"image\":\"textures/items/gold_ingot\"},{\"order\":\"34\",\"name\":\"青金石\",\"id\":\"dye\",\"damage\":\"4\",\"regex\":\"\\\\{\\\"ck\\\":\\\"Damage\\\",\\\"cv\\\":\\\\{\\\"tt\\\":2,\\\"tv\\\":4\\\\}\\\\}\",\"award\":80,\"image\":\"textures/items/dye_powder_blue\"},{\"order\":\"35\",\"name\":\"红石\",\"id\":\"redstone\",\"damage\":\"-1\",\"regex\":\"\",\"award\":20,\"image\":\"textures/items/redstone_dust\"},{\"order\":\"36\",\"name\":\"钻石\",\"id\":\"diamond\",\"damage\":\"-1\",\"regex\":\"\",\"award\":200,\"image\":\"textures/items/diamond\"},{\"order\":\"37\",\"name\":\"绿宝石\",\"id\":\"emerald\",\"damage\":\"-1\",\"regex\":\"\",\"award\":60,\"image\":\"textures/items/emerald\"}]},{\"type\":\"杂物/生活用品\",\"order\":\"46\",\"image\":false,\"content\":[{\"order\":\"47\",\"name\":\"马鞍\",\"id\":\"saddle\",\"damage\":\"-1\",\"regex\":\"\",\"award\":300,\"image\":\"textures/items/saddle\"},{\"order\":\"48\",\"name\":\"末影珍珠\",\"id\":\"ender_pearl\",\"damage\":\"-1\",\"regex\":\"\",\"award\":200,\"image\":\"textures/items/ender_pearl\"}]}],\"sell\":[{\"type\":\"基础方块\",\"order\":\"1\",\"image\":\"textures/blocks/grass_side_carried\",\"content\":[{\"order\":\"2\",\"name\":\"泥土\",\"id\":\"dirt\",\"damage\":\"0\",\"price\":\"2.00\",\"image\":\"textures/blocks/dirt\"},{\"type\":\"各种木头\",\"order\":\"18\",\"image\":\"textures/blocks/log_oak\",\"content\":[{\"order\":\"20\",\"name\":\"橡木原木\",\"id\":\"log\",\"damage\":\"1\",\"price\":\"5.00\",\"image\":\"textures/blocks/log_oak\"},{\"order\":\"21\",\"name\":\"云杉原木\",\"id\":\"log\",\"damage\":\"2\",\"price\":\"7.00\",\"image\":\"textures/blocks/log_spruce\"},{\"order\":\"22\",\"name\":\"白桦原木\",\"id\":\"log\",\"damage\":\"3\",\"price\":\"6.00\",\"image\":\"textures/blocks/log_birch\"},{\"order\":\"23\",\"name\":\"丛林原木\",\"id\":\"log\",\"damage\":\"4\",\"price\":\"8.00\",\"image\":\"textures/blocks/log_jungle\"},{\"order\":\"24\",\"name\":\"金合欢原木\",\"id\":\"log2\",\"damage\":\"1\",\"price\":\"9.00\",\"image\":\"textures/blocks/log_acacia\"},{\"order\":\"25\",\"name\":\"深色橡木原木\",\"id\":\"log2\",\"damage\":\"2\",\"price\":\"7.50\",\"image\":\"textures/blocks/log_big_oak\"}]},{\"order\":\"47\",\"name\":\"马鞍\",\"id\":\"saddle\",\"damage\":\"0\",\"price\":\"500.00\",\"image\":\"textures/items/saddle\"},{\"order\":\"48\",\"name\":\"末影珍珠\",\"id\":\"ender_pearl\",\"damage\":\"0\",\"price\":\"1000.00\",\"image\":\"textures/items/ender_pearl\"},{\"order\":\"49\",\"name\":\"海洋之心\",\"id\":\"heart_of_the_sea\",\"damage\":\"0\",\"price\":\"6666.67\",\"image\":\"textures/items/heartofthesea_closed\"}]},{\"type\":\"建筑党专用\",\"order\":\"81\",\"image\":\"textures/blocks/brick\",\"content\":[{\"type\":\"陶土类\",\"order\":\"82\",\"image\":false,\"content\":[{\"order\":\"83\",\"name\":\"白色陶瓦\",\"id\":\"stained_hardened_clay\",\"damage\":\"0\",\"price\":\"3.00\",\"image\":\"textures/blocks/hardened_clay_stained_white\"},{\"order\":\"84\",\"name\":\"橙色陶瓦\",\"id\":\"stained_hardened_clay\",\"damage\":\"1\",\"price\":\"3.00\",\"image\":\"textures/blocks/hardened_clay_stained_orange\"},{\"order\":\"85\",\"name\":\"品红色陶瓦\",\"id\":\"stained_hardened_clay\",\"damage\":\"2\",\"price\":\"3.00\",\"image\":\"textures/blocks/hardened_clay_stained_magenta\"},{\"order\":\"86\",\"name\":\"淡蓝色陶瓦\",\"id\":\"stained_hardened_clay\",\"damage\":\"3\",\"price\":\"3.00\",\"image\":\"textures/blocks/hardened_clay_stained_light_blue\"},{\"order\":\"87\",\"name\":\"黄色陶瓦\",\"id\":\"stained_hardened_clay\",\"damage\":\"4\",\"price\":\"3.00\",\"image\":\"textures/blocks/hardened_clay_stained_yellow\"},{\"order\":\"88\",\"name\":\"黄绿色陶瓦\",\"id\":\"stained_hardened_clay\",\"damage\":\"5\",\"price\":\"3.00\",\"image\":\"textures/blocks/hardened_clay_stained_lime\"},{\"order\":\"89\",\"name\":\"粉红色陶瓦\",\"id\":\"stained_hardened_clay\",\"damage\":\"6\",\"price\":\"3.00\",\"image\":\"textures/blocks/hardened_clay_stained_pink\"},{\"order\":\"90\",\"name\":\"灰色陶瓦\",\"id\":\"stained_hardened_clay\",\"damage\":\"7\",\"price\":\"3.00\",\"image\":\"textures/blocks/hardened_clay_stained_gray\"},{\"order\":\"91\",\"name\":\"淡灰色陶瓦\",\"id\":\"stained_hardened_clay\",\"damage\":\"8\",\"price\":\"3.00\",\"image\":\"textures/blocks/hardened_clay_stained_silver\"},{\"order\":\"92\",\"name\":\"青色陶瓦\",\"id\":\"stained_hardened_clay\",\"damage\":\"9\",\"price\":\"3.00\",\"image\":\"textures/blocks/hardened_clay_stained_cyan\"},{\"order\":\"93\",\"name\":\"紫色陶瓦\",\"id\":\"stained_hardened_clay\",\"damage\":\"10\",\"price\":\"3.00\",\"image\":\"textures/blocks/hardened_clay_stained_purple\"},{\"order\":\"94\",\"name\":\"蓝色陶瓦\",\"id\":\"stained_hardened_clay\",\"damage\":\"11\",\"price\":\"3.00\",\"image\":\"textures/blocks/hardened_clay_stained_blue\"},{\"order\":\"95\",\"name\":\"棕色陶瓦\",\"id\":\"stained_hardened_clay\",\"damage\":\"12\",\"price\":\"3.00\",\"image\":\"textures/blocks/hardened_clay_stained_brown\"},{\"order\":\"96\",\"name\":\"绿色陶瓦\",\"id\":\"stained_hardened_clay\",\"damage\":\"13\",\"price\":\"3.00\",\"image\":\"textures/blocks/hardened_clay_stained_green\"},{\"order\":\"97\",\"name\":\"红色陶瓦\",\"id\":\"stained_hardened_clay\",\"damage\":\"14\",\"price\":\"3.00\",\"image\":\"textures/blocks/hardened_clay_stained_red\"},{\"order\":\"98\",\"name\":\"黑色陶瓦\",\"id\":\"stained_hardened_clay\",\"damage\":\"15\",\"price\":\"3.00\",\"image\":\"textures/blocks/hardened_clay_stained_black\"},{\"order\":\"99\",\"name\":\"陶瓦\",\"id\":\"\\r\\nhardened_clay\",\"damage\":\"0\",\"price\":\"3.00\",\"image\":\"textures/blocks/hardened_clay\"}]},{\"type\":\"羊毛类\",\"order\":\"101\",\"image\":false,\"content\":[{\"order\":\"102\",\"name\":\"白色羊毛\",\"id\":\"wool\",\"damage\":\"0\",\"price\":\"3.00\",\"image\":\"textures/blocks/wool_colored_white\"},{\"order\":\"103\",\"name\":\"橙色羊毛\",\"id\":\"wool\",\"damage\":\"1\",\"price\":\"3.00\",\"image\":\"textures/blocks/wool_colored_orange\"},{\"order\":\"104\",\"name\":\"品红色羊毛\",\"id\":\"wool\",\"damage\":\"2\",\"price\":\"3.00\",\"image\":\"textures/blocks/wool_colored_magenta\"},{\"order\":\"105\",\"name\":\"淡蓝色羊毛\",\"id\":\"wool\",\"damage\":\"3\",\"price\":\"3.00\",\"image\":\"textures/blocks/wool_colored_light_blue\"},{\"order\":\"106\",\"name\":\"黄色羊毛\",\"id\":\"wool\",\"damage\":\"4\",\"price\":\"3.00\",\"image\":\"textures/blocks/wool_colored_yellow\"},{\"order\":\"107\",\"name\":\"黄绿色羊毛\",\"id\":\"wool\",\"damage\":\"5\",\"price\":\"3.00\",\"image\":\"textures/blocks/wool_colored_lime\"},{\"order\":\"108\",\"name\":\"粉红色羊毛\",\"id\":\"wool\",\"damage\":\"6\",\"price\":\"3.00\",\"image\":\"textures/blocks/wool_colored_pink\"},{\"order\":\"109\",\"name\":\"灰色羊毛\",\"id\":\"wool\",\"damage\":\"7\",\"price\":\"3.00\",\"image\":\"textures/blocks/wool_colored_gray\"},{\"order\":\"110\",\"name\":\"淡灰色羊毛\",\"id\":\"wool\",\"damage\":\"8\",\"price\":\"3.00\",\"image\":\"textures/blocks/wool_colored_silver\"},{\"order\":\"111\",\"name\":\"青色羊毛\",\"id\":\"wool\",\"damage\":\"9\",\"price\":\"3.00\",\"image\":\"textures/blocks/wool_colored_cyan\"},{\"order\":\"112\",\"name\":\"紫色羊毛\",\"id\":\"wool\",\"damage\":\"10\",\"price\":\"3.00\",\"image\":\"textures/blocks/wool_colored_purple\"},{\"order\":\"113\",\"name\":\"蓝色羊毛\",\"id\":\"wool\",\"damage\":\"11\",\"price\":\"3.00\",\"image\":\"textures/blocks/wool_colored_blue\"},{\"order\":\"114\",\"name\":\"棕色羊毛\",\"id\":\"wool\",\"damage\":\"12\",\"price\":\"3.00\",\"image\":\"textures/blocks/wool_colored_brown\"},{\"order\":\"115\",\"name\":\"绿色羊毛\",\"id\":\"wool\",\"damage\":\"13\",\"price\":\"3.00\",\"image\":\"textures/blocks/wool_colored_green\"},{\"order\":\"116\",\"name\":\"红色羊毛\",\"id\":\"wool\",\"damage\":\"14\",\"price\":\"3.00\",\"image\":\"textures/blocks/wool_colored_red\"},{\"order\":\"117\",\"name\":\"黑色羊毛\",\"id\":\"wool\",\"damage\":\"15\",\"price\":\"3.00\",\"image\":\"textures/blocks/wool_colored_black\"}]},{\"type\":\"混泥土\",\"order\":\"119\",\"image\":false,\"content\":[{\"order\":\"120\",\"name\":\"白色混凝土\",\"id\":\"concrete\",\"damage\":\"0\",\"price\":\"3.00\",\"image\":\"textures/blocks/concrete_white\"},{\"order\":\"121\",\"name\":\"橙色混凝土\",\"id\":\"concrete\",\"damage\":\"1\",\"price\":\"3.00\",\"image\":\"textures/blocks/concrete_orange\"},{\"order\":\"122\",\"name\":\"品红色混凝土\",\"id\":\"concrete\",\"damage\":\"2\",\"price\":\"3.00\",\"image\":\"textures/blocks/concrete_magenta\"},{\"order\":\"123\",\"name\":\"淡蓝色混凝土\",\"id\":\"concrete\",\"damage\":\"3\",\"price\":\"3.00\",\"image\":\"textures/blocks/concrete_light_blue\"},{\"order\":\"124\",\"name\":\"黄色混凝土\",\"id\":\"concrete\",\"damage\":\"4\",\"price\":\"3.00\",\"image\":\"textures/blocks/concrete_yellow\"},{\"order\":\"125\",\"name\":\"黄绿色混凝土\",\"id\":\"concrete\",\"damage\":\"5\",\"price\":\"3.00\",\"image\":\"textures/blocks/concrete_lime\"},{\"order\":\"126\",\"name\":\"粉红色混凝土\",\"id\":\"concrete\",\"damage\":\"6\",\"price\":\"3.00\",\"image\":\"textures/blocks/concrete_pink\"},{\"order\":\"127\",\"name\":\"灰色混凝土\",\"id\":\"concrete\",\"damage\":\"7\",\"price\":\"3.00\",\"image\":\"textures/blocks/concrete_gray\"},{\"order\":\"128\",\"name\":\"淡灰色混凝土\",\"id\":\"concrete\",\"damage\":\"8\",\"price\":\"3.00\",\"image\":\"textures/blocks/concrete_silver\"},{\"order\":\"129\",\"name\":\"青色混凝土\",\"id\":\"concrete\",\"damage\":\"9\",\"price\":\"3.00\",\"image\":\"textures/blocks/concrete_cyan\"},{\"order\":\"130\",\"name\":\"紫色混凝土\",\"id\":\"concrete\",\"damage\":\"10\",\"price\":\"3.00\",\"image\":\"textures/blocks/concrete_purple\"},{\"order\":\"131\",\"name\":\"蓝色混凝土\",\"id\":\"concrete\",\"damage\":\"11\",\"price\":\"3.00\",\"image\":\"textures/blocks/concrete_blue\"},{\"order\":\"132\",\"name\":\"棕色混凝土\",\"id\":\"concrete\",\"damage\":\"12\",\"price\":\"3.00\",\"image\":\"textures/blocks/concrete_brown\"},{\"order\":\"133\",\"name\":\"绿色混凝土\",\"id\":\"concrete\",\"damage\":\"13\",\"price\":\"3.00\",\"image\":\"textures/blocks/concrete_green\"},{\"order\":\"134\",\"name\":\"红色混凝土\",\"id\":\"concrete\",\"damage\":\"14\",\"price\":\"3.00\",\"image\":\"textures/blocks/concrete_red\"},{\"order\":\"135\",\"name\":\"黑色混凝土\",\"id\":\"concrete\",\"damage\":\"15\",\"price\":\"3.00\",\"image\":\"textures/blocks/concrete_black\"}]}]}]}";

    }
}
