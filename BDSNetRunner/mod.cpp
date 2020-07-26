#include "mod.h"
#include "THook.h"
#include <stdio.h>
#include <iostream>
#include "BDS.hpp"
#include <thread>
#include <map>
#include <fstream>
#include <mscoree.h>
#include <metahost.h>
#include "json/json.h"
#include "tick/tick.h"
#include "GUI/SimpleForm.h"
#include <mutex>
#include "commands/commands.h"
#pragma comment(lib, "mscoree.lib")

// ��ǰ���ƽ̨�汾��
static const wchar_t* VERSION = L"1.16.1.2";
static const wchar_t* ISFORCOMMERCIAL = L"1";

static bool netregok = false;

static void releaseNetFramework();


static std::mutex mleftlock;

// ������Ϣ
template<typename T>
static void PR(T arg) {
#ifndef RELEASED
	std::cout << arg << std::endl;
#endif // !RELEASED
}

// ת��Json����Ϊ�ַ���
static std::string toJsonString(Json::Value v) {
	Json::StreamWriterBuilder w;
	std::ostringstream os;
	std::unique_ptr<Json::StreamWriter> jsonWriter(w.newStreamWriter());
	jsonWriter->write(v, &os);
	return std::string(os.str());
}

// ת���ַ���ΪJson����
static Json::Value toJson(std::string s) {
	Json::Value jv;
	Json::CharReaderBuilder r;
	JSONCPP_STRING errs;
	std::unique_ptr<Json::CharReader> const jsonReader(r.newCharReader());
	bool res = jsonReader->parse(s.c_str(), s.c_str() + s.length(), &jv, &errs);
	if (!res || !errs.empty()) {
		PR(u8"JSONת��ʧ�ܡ���" + errs);
	}
	return jv;
}

// UTF-8 ת GBK
static std::string UTF8ToGBK(const char* strUTF8)
{
	int len = MultiByteToWideChar(CP_UTF8, 0, strUTF8, -1, NULL, 0);
	wchar_t* wszGBK = new wchar_t[len + 1];
	memset(wszGBK, 0, len * 2 + 2);
	MultiByteToWideChar(CP_UTF8, 0, strUTF8, -1, wszGBK, len);
	len = WideCharToMultiByte(CP_ACP, 0, wszGBK, -1, NULL, 0, NULL, NULL);
	char* szGBK = new char[len + 1];
	memset(szGBK, 0, len + 1);
	WideCharToMultiByte(CP_ACP, 0, wszGBK, -1, szGBK, len, NULL, NULL);
	std::string strTemp(szGBK);
	if (wszGBK) delete[] wszGBK;
	if (szGBK) delete[] szGBK;
	return strTemp;
}

// GBK ת UTF-8
static std::string GBKToUTF8(const char* strGBK)
{
	std::string strOutUTF8 = "";
	WCHAR* str1;
	int n = MultiByteToWideChar(CP_ACP, 0, strGBK, -1, NULL, 0);
	str1 = new WCHAR[n];
	MultiByteToWideChar(CP_ACP, 0, strGBK, -1, str1, n);
	n = WideCharToMultiByte(CP_UTF8, 0, str1, -1, NULL, 0, NULL, NULL);
	char* str2 = new char[n];
	WideCharToMultiByte(CP_UTF8, 0, str1, -1, str2, n, NULL, NULL);
	strOutUTF8 = str2;
	delete[]str1;
	str1 = NULL;
	delete[]str2;
	str2 = NULL;
	return strOutUTF8;
}

// �Զ������ַ�
static void autoByteCpy(char** d, const char* s) {
	if (d) {
		VA l = strlen(s);
		if (*d) {
			delete (*d);
		}
		*d = new char[l + 1]{ 0 };
		strcpy_s(*d, l + 1, s);
	}
}

static VA p_spscqueue = 0;

static VA p_level = 0;

static VA p_ServerNetworkHandle = 0;

static HMODULE GetSelfModuleHandle()
{
	MEMORY_BASIC_INFORMATION mbi;
	return ((::VirtualQuery(GetSelfModuleHandle, &mbi, sizeof(mbi)) != 0) ? (HMODULE)mbi.AllocationBase : NULL);
}
// ��ȡ����DLL·��
static std::wstring GetDllPathandVersion() {
	std::ifstream file;
	wchar_t curDir[256]{ 0 };
	GetModuleFileName(GetSelfModuleHandle(), curDir, 256);
	std::wstring dllandVer = std::wstring(curDir);
	dllandVer = dllandVer + std::wstring(L",") + std::wstring(VERSION);
	if (netregok)
		dllandVer = dllandVer + std::wstring(L",") + std::wstring(ISFORCOMMERCIAL);
	return dllandVer;
}

std::unordered_map<std::string, std::vector<void*>*> beforecallbacks, aftercallbacks;

// ִ�лص�
static bool runCscode(std::string key, ActMode mode, Events& eventData) {
	auto& funcs = (mode == ActMode::BEFORE) ? beforecallbacks :
		aftercallbacks;
	auto dv = funcs[key];
	bool ret = true;
	if (dv) {
		if (dv->size() > 0) {
			for (auto& func : *dv) {
				try {
					ret = ret && ((bool(*)(Events))func)(eventData);
				}
				catch (...) { PR("[CSR] Event callback exception."); }
			}
		}
	}
	return ret;
}

static ACTEVENT ActEvent;

static bool addListener(std::string key, ActMode mode, bool(*func)(Events)) {
	auto& funcs = (mode == ActMode::BEFORE) ? beforecallbacks :
		aftercallbacks;
	if (key != "" && func != NULL) {
		auto dv = funcs[key];
		if (dv == NULL) {
			dv = new std::vector<void*>();
			funcs[key] = dv;
		}
		if (std::find(dv->begin(), dv->end(), func) == dv->end()) {
			// δ�ҵ����к���������ص�
			dv->push_back(func);
			return true;
		}
	}
	return false;
}

static void remove_v2(std::vector<void*>* v, void* val) {
	v->erase(std::remove(v->begin(), v->end(), val), v->end());
}

static bool removeListener(std::string key, ActMode mode, bool(*func)(Events)) {
	auto& funcs = (mode == ActMode::BEFORE) ? beforecallbacks :
		aftercallbacks;
	if (key != "" && func != NULL) {
		auto dv = funcs[key];
		if (dv != NULL) {
			bool exi = std::find(dv->begin(), dv->end(), func) != dv->end();
			if (exi)
				remove_v2(dv, func);
			return exi;
		}
	}
	return false;
}

bool addBeforeActListener(const char* key, bool(*func)(Events))
{
	return addListener(key, ActMode::BEFORE, func);
}

bool addAfterActListener(const char* key, bool(*func)(Events))
{
	return addListener(key, ActMode::AFTER, func);
}

bool removeBeforeActListener(const char* key, bool(*func)(Events)) {
	return removeListener(key, ActMode::BEFORE, func);
}

bool removeAfterActListener(const char* key, bool(*func)(Events)) {
	return removeListener(key, ActMode::AFTER, func);
}

static std::unordered_map<std::string, void*> shareData;

void setSharePtr(const char* key, void* func)
{
	shareData[key] = func;
}

void* getSharePtr(const char* key) {
	return shareData[key];
}

void* removeSharePtr(const char* key) {
	std::string k = std::string(key);
	void* x = shareData[k];
	shareData.erase(k);
	return x;
}



// ά��IDת��Ϊ�����ַ�
static std::string toDimenStr(int dimensionId) {
	switch (dimensionId) {
	case 0:return u8"������";
	case 1:return u8"����";
	case 2:return u8"ĩ��";
	default:
		break;
	}
	return u8"δ֪ά��";
}


static VA regHandle = 0;

static struct CmdDescriptionFlags {
	std::string description;
	char level;
	char flag1;
	char flag2;
};

static std::unordered_map<std::string, std::unique_ptr<CmdDescriptionFlags>> cmddescripts;

// ��������setCommandDescribeEx
// ���ܣ�����һ��ȫ��ָ��˵��
// ����������2��
// �������ͣ��ַ������ַ��������ͣ����ͣ�����
// ������⣺cmd - ���description - ����˵����level - ִ��Ҫ��ȼ���flag1 - ��������1�� flag2 - ��������2
// ��ע������ע�����������ܲ���ı�ͻ��˽���
void setCommandDescribeEx(const char* cmd, const char* description, char level, char flag1, char flag2) {
	auto strcmd = GBKToUTF8(cmd);
	if (strcmd.length()) {
		auto flgs = std::make_unique<CmdDescriptionFlags>();
		cmddescripts[strcmd] = std::move(flgs);
		if (regHandle) {
			std::string c = strcmd;
			auto ct = GBKToUTF8(description);
			SYMCALL(VA, MSSYM_MD5_8574de98358ff66b5a913417f44dd706, regHandle, &c, ct.c_str(), level, flag1, flag2);
		}
	}
}

// ��������runcmd
// ���ܣ�ִ�к�ָ̨��
// ����������1��
// �������ͣ��ַ���
// ������⣺cmd - �﷨��ȷ��MCָ��
// ����ֵ���Ƿ�����ִ��
bool runcmd(const char* cmd) {
	auto strcmd = GBKToUTF8(cmd);
	if (p_spscqueue != 0) {
		if (p_level) {
			auto fr = [strcmd]() {
				SYMCALL(bool, MSSYM_MD5_b5c9e566146b3136e6fb37f0c080d91e, p_spscqueue, strcmd);
			};
			safeTick(fr);
			return true;
		}
	}
	return false;
}

// ��׼����������
static const VA STD_COUT_HANDLE = SYM_OBJECT(VA,
	MSSYM_B2UUA3impB2UQA4coutB1AA3stdB2AAA23VB2QDA5basicB1UA7ostreamB1AA2DUB2QDA4charB1UA6traitsB1AA1DB1AA3stdB3AAAA11B1AA1A);

// ��������logout
// ���ܣ�����һ�����������Ϣ���ɱ����أ�
// ����������1��
// �������ͣ��ַ���
// ������⣺cmdout - �����͵���������ַ���
void logout(const char* cmdout) {
	std::string strout = GBKToUTF8(cmdout) + "\n";
	SYMCALL(VA, MSSYM_MD5_b5f2f0a753fc527db19ac8199ae8f740, STD_COUT_HANDLE, strout.c_str(), strout.length());
}

static std::unordered_map<std::string, Player*> onlinePlayers;
static std::unordered_map<Player*, bool> playerSign;

// ��������getOnLinePlayers
// ���ܣ���ȡ��������б�
// ����������0��
// ����ֵ������б��Json�ַ���
const char* getOnLinePlayers() {
	Json::Value rt;
	Json::Value jv;
	mleftlock.lock();
	for (auto& op : playerSign) {
		Player* p = op.first;
		if (op.second) {
			jv["playername"] = p->getNameTag();
			jv["uuid"] = p->getUuid()->toString();
			jv["xuid"] = p->getXuid(p_level);
			rt.append(jv);
		}
	}
	mleftlock.unlock();
	const char* jstr = rt.toStyledString().c_str();
	VA l = strlen(jstr);
	std::unique_ptr<char[]> x = std::unique_ptr<char[]>(new char[l + 1]);
	strcpy_s(*(char**)&x, l + 1, jstr);
	return *(const char**)&x;
}

#if (COMMERCIAL)

// ������������Ϊ�����µ���ƫ����
static void countPos2AOff(BPos3* a1, BPos3* a2) {
	int minx, miny, minz, dx, dy, dz;
	minx = min(a1->x, a2->x);
	miny = min(a1->y, a2->y);
	minz = min(a1->z, a2->z);
	dx = abs(a1->x - a2->x);
	dy = abs(a1->y - a2->y);
	dz = abs(a1->z - a2->z);
	a1->x = minx;
	a1->y = miny;
	a1->z = minz;
	a2->x = dx + 1;
	a2->y = dy + 1;
	a2->z = dz + 1;
}

// ��������getStructure
// ���ܣ���ȡһ���ṹ
// ����������5��
// �������ͣ����ͣ��ַ������ַ����������ͣ�������
// ������⣺dimensionid - ��ͼά�ȣ�posa - ����JSON�ַ�����posb - ����JSON�ַ�����exent - �Ƿ񵼳�ʵ�壬exblk - �Ƿ񵼳�����
// ����ֵ���ṹjson�ַ���
static const char* getStructure(int did, const char* jsonposa, const char* jsonposb, bool exent, bool exblk) {
	std::unique_ptr<char[]> x;
	if (p_level && (did > -1 && did < 3)) {
		Json::Value jposa = toJson(jsonposa);
		Json::Value jposb = toJson(jsonposb);
		BPos3 a, b;
		if (!jposa.isNull() && !jposb.isNull()) {
			a.x = jposa["x"].asInt();
			a.y = jposa["y"].asInt();
			a.z = jposa["z"].asInt();
			b.x = jposb["x"].asInt();
			b.y = jposb["y"].asInt();
			b.z = jposb["z"].asInt();
			countPos2AOff(&a, &b);
			VA t = StructureTemplate::getStructure(p_level, did, a, b, exent, exblk);
			std::string ret = (*(Tag**)t)->toJson().toStyledString();
			(*(Tag**)t)->clearAll();
			*(VA*)t = 0;
			delete (VA*)t;
			const char* jstr = ret.c_str();
			VA l = strlen(jstr);
			x = std::unique_ptr<char[]>(new char[l + 1]);
			strcpy_s(*(char**)&x, l + 1, jstr);
			return *(const char**)&x;
		}
	}
	return NULL;
}

// ��������setStructure
// ���ܣ�����һ���ṹ��ָ��λ��
// ����������6��
// �������ͣ��ַ��������ͣ��ַ��������ͣ������ͣ�������
// ������⣺strnbt - �ṹJSON�ַ�����dimensionid - ��ͼά�ȣ�posa - ��ʼ������JSON�ַ�����rot - ��ת���ͣ�exent - �Ƿ���ʵ�壬exblk - �Ƿ��뷽��
// ����ֵ���Ƿ����óɹ�
static bool setStructure(const char* jdata, int did, const char* jsonposa, char rot, bool exent, bool exblk) {
	bool ret = false;
	if (p_level && (did > -1 && did < 3)) {
		Json::Value jposa = toJson(jsonposa);
		BPos3 a;
		if (!jposa.isNull()) {
			a.x = jposa["x"].asInt();
			a.y = jposa["y"].asInt();
			a.z = jposa["z"].asInt();
			Json::Value jv = toJson(jdata);
			VA t = Tag::fromJson(jv);
			ret = StructureTemplate::placeStructure(*(VA*)t, p_level, did, a, rot, exent, exblk);
			(*(Tag**)t)->clearAll();
			*(VA*)t = 0;
			delete (VA*)t;
		}
	}
	return ret;
}

// ��ȡ��׼����ֵ�б�
static Json::Value getAbilities(Player* p) {
	Json::Value jv;
	for (int i = 0; i < 18; i++) {
		std::string x = Abilities::getAbilityName(i);
		if (x != "") {
			jv[x] = p->getAbility(i);
		}
	}
	return jv;
}

// ��������getPlayerAbilities
// ���ܣ���ȡ���������
// ����������1��
// �������ͣ��ַ���
// ������⣺uuid - ������ҵ�uuid�ַ���
// ����ֵ������json�ַ���
static const char* getPlayerAbilities(const char* uuid) {
	std::string ret = "";
	Player* p = onlinePlayers[uuid];
	if (playerSign[p]) {
		mleftlock.lock();
		auto jv = getAbilities(p);
		mleftlock.unlock();
		const char* ret = jv.toStyledString().c_str();
		VA l = strlen(ret);
		std::unique_ptr<char[]> x = std::unique_ptr<char[]>(new char[l + 1]);
		strcpy_s(*(char**)&x, l + 1, ret);
		return *(const char**)&x;
	}
	return NULL;
}

// ��������setPlayerAbilities
// ���ܣ��������������
// ����������2��
// �������ͣ��ַ������ַ���
// ������⣺uuid - ������ҵ�uuid�ַ�����newAbilities - ������json�����ַ���
// ����ֵ���Ƿ����óɹ�
static bool setPlayerAbilities(const char* uuid, const char* abdata) {
	Player* p = onlinePlayers[uuid];
	if (playerSign[p]) {
		std::string suuid = uuid;
		std::string sabdata = abdata;
		auto fr = [suuid, sabdata]() {
			Player* p = onlinePlayers[suuid];
			if (playerSign[p]) {
				Json::Value jv = toJson(sabdata);
				if (jv.isNull()) {
					return;
				}
				auto members = jv.getMemberNames();
				for (auto& a : members) {
					p->setAbility(Abilities::nameToAbilityIndex(a.c_str()), jv[a].asBool());
				}
				p->setPermission(p->getPermission());
			}
		};
		safeTick(fr);
		return true;
	}
	return false;
}

// ��������getPlayerAttributes
// ���ܣ���ȡ������Ա�
// ����������1��
// �������ͣ��ַ���
// ������⣺uuid - ������ҵ�uuid�ַ���
// ����ֵ������json�ַ���
static const char* getPlayerAttributes(const char* uuid) {
	Player* p = onlinePlayers[uuid];
	if (playerSign[p]) {
		mleftlock.lock();
		const std::map<std::string, VA>& al = Mob::getAttrs();
		Json::Value jv;
		for (auto& k : al) {
			jv[k.first] = p->getAttr(k.first.c_str());
		}
		mleftlock.unlock();
		const char* ret = jv.toStyledString().c_str();
		VA l = strlen(ret);
		std::unique_ptr<char[]> x = std::unique_ptr<char[]>(new char[l + 1]);
		strcpy_s(*(char**)&x, l + 1, ret);
		return *(const char**)&x;
	}
	return NULL;
}

// ��������setPlayerTempAttributes
// ���ܣ��������������ʱֵ��
// ����������2��
// �������ͣ��ַ������ַ���
// ������⣺uuid - ������ҵ�uuid�ַ�����newTempAttributes - ��������ʱֵjson�����ַ���
// ����ֵ���Ƿ����óɹ�
// ����ע���ú������ܲ������ͻ���ʵ����ʾֵ��
static bool setPlayerTempAttributes(const char* uuid, const char* jstr) {
	Player* p = onlinePlayers[uuid];
	if (playerSign[p]) {
		std::string suuid = uuid;
		std::string sjstr = jstr;
		auto fr = [suuid, sjstr]() {
			Player* p = onlinePlayers[suuid];
			if (playerSign[p]) {
				Json::Value jv = toJson(sjstr);
				if (jv.isNull()) {
					return;
				}
				auto members = jv.getMemberNames();
				for (auto& a : members) {
					p->setTmpAttr(a.c_str(), jv[a].asFloat());
				}
			}
		};
		safeTick(fr);
		return true;
	}
	return false;
}

// ��������getPlayerMaxAttributes
// ���ܣ���ȡ�����������ֵ��
// ����������1��
// �������ͣ��ַ���
// ������⣺uuid - ������ҵ�uuid�ַ���
// ����ֵ����������ֵjson�ַ���
static const char* getPlayerMaxAttributes(const char* uuid) {
	Player* p = onlinePlayers[uuid];
	if (playerSign[p]) {
		mleftlock.lock();
		const std::map<std::string, VA>& al = Mob::getMaxAttrs();
		Json::Value jv;
		for (auto& k : al) {
			jv[k.first] = p->getMaxAttr(k.first.c_str());
		}
		mleftlock.unlock();
		const char* ret = jv.toStyledString().c_str();
		VA l = strlen(ret);
		std::unique_ptr<char[]> x = std::unique_ptr<char[]>(new char[l + 1]);
		strcpy_s(*(char**)&x, l + 1, ret);
		return *(const char**)&x;
	}
	return NULL;
}

// ��������setPlayerMaxAttributes
// ���ܣ����������������ֵ��
// ����������2��
// �������ͣ��ַ������ַ���
// ������⣺uuid - ������ҵ�uuid�ַ�����newMaxAttributes - ����������ֵjson�����ַ���
// ����ֵ���Ƿ����óɹ�
// ����ע���ú������ܲ������ͻ���ʵ����ʾֵ��
static bool setPlayerMaxAttributes(const char* uuid, const char* jstr) {
	Player* p = onlinePlayers[uuid];
	if (playerSign[p]) {
		std::string suuid = uuid;
		std::string sjstr = jstr;
		auto fr = [suuid, sjstr]() {
			Player* p = onlinePlayers[suuid];
			if (playerSign[p]) {
				Json::Value jv = toJson(sjstr);
				if (jv.isNull()) {
					return;
				}
				auto members = jv.getMemberNames();
				for (auto& a : members) {
					p->setMaxAttr(a.c_str(), jv[a].asFloat());
				}
			}
		};
		safeTick(fr);
		return true;
	}
	return false;
}

// ��������getPlayerItems
// ���ܣ���ȡ���������Ʒ�б�
// ����������1��
// �������ͣ��ַ���
// ������⣺uuid - ������ҵ�uuid�ַ���
// ����ֵ����Ʒ�б�json�ַ���
static const char* getPlayerItems(const char* uuid) {
	Player* p = onlinePlayers[uuid];
	if (playerSign[p]) {
		mleftlock.lock();
		Json::Value jv = p->getAllItemsList();
		mleftlock.unlock();
		const char* ret = jv.toStyledString().c_str();
		VA l = strlen(ret);
		std::unique_ptr<char[]> x = std::unique_ptr<char[]>(new char[l + 1]);
		strcpy_s(*(char**)&x, l + 1, ret);
		return *(const char**)&x;
	}
	return NULL;
}

// ��������setPlayerItems
// ���ܣ��������������Ʒ�б�
// ����������2��
// �������ͣ��ַ������ַ���
// ������⣺uuid - ������ҵ�uuid�ַ�����newItems - ����Ʒ�б�json�����ַ���
// ����ֵ���Ƿ����óɹ�
// ����ע���ض������¿��ܲ�������Ϸ��ʵ����Ʒ��
static bool setPlayerItems(const char* uuid, const char* jstr) {
	Player* p = onlinePlayers[uuid];
	if (playerSign[p]) {
		std::string suuid = uuid;
		std::string sjstr = GBKToUTF8(jstr);
		auto fr = [suuid, sjstr]() {
			Player* p = onlinePlayers[suuid];
			if (playerSign[p]) {
				Json::Value jv = toJson(sjstr);
				if (jv.isNull()) {
					return;
				}
				p->setAllItemsList(jv);
				p->updateInventory();
			}
		};
		safeTick(fr);
		return true;
	}
	return false;
}

// ��������getPlayerSelectedItem
// ���ܣ���ȡ��ҵ�ǰѡ������Ϣ
// ����������1��
// �������ͣ��ַ���
// ������⣺uuid - ������ҵ�uuid�ַ���
// ����ֵ����ǰѡ������Ϣjson�ַ���
static const char* getPlayerSelectedItem(const char* uuid) {
	Player* p = onlinePlayers[uuid];
	if (playerSign[p]) {
		mleftlock.lock();
		Json::Value jv;
		jv["selectedslot"] = p->getSelectdItemSlot();
		ItemStack* its = (ItemStack*)p->getSelectedItem();
		if (its != SYM_POINT(ItemStack, MSSYM_B1QA5EMPTYB1UA4ITEMB1AA9ItemStackB2AAA32V1B1AA1B)) {
			jv["selecteditem"] = its->toJson();
		}
		mleftlock.unlock();
		const char* ret = jv.toStyledString().c_str();
		VA l = strlen(ret);
		std::unique_ptr<char[]> x = std::unique_ptr<char[]>(new char[l + 1]);
		strcpy_s(*(char**)&x, l + 1, ret);
		return *(const char**)&x;
	}
	return NULL;
}

// ��������addPlayerItemEx
// ���ܣ��������һ����Ʒ
// ����������2��
// �������ͣ��ַ������ַ���
// ������⣺uuid - ������ҵ�uuid�ַ�����item - ��Ʒjson�����ַ���
// ����ֵ���Ƿ���ӳɹ�
// ����ע���ض������¿��ܲ�������Ϸ��ʵ����Ʒ��
static bool addPlayerItemEx(const char* uuid, const char* item) {
	Player* p = onlinePlayers[uuid];
	if (playerSign[p]) {
		std::string suuid = uuid;
		std::string sitem = GBKToUTF8(item);
		auto fr = [suuid, sitem]() {
			Player* p = onlinePlayers[suuid];
			if (playerSign[p]) {
				Json::Value jv = toJson(sitem);
				if (jv.isNull()) {
					return;
				}
				ItemStack x;
				x.fromJson(jv);
				p->addItem((VA)&x);
				p->updateInventory();
			}
		};
		safeTick(fr);
		return true;
	}
	return false;
}

// ��������getPlayerEffects
// ���ܣ���ȡ�������Ч���б�
// ����������1��
// �������ͣ��ַ���
// ������⣺uuid - ������ҵ�uuid�ַ���
// ����ֵ��Ч���б�json�ַ���
static const char* getPlayerEffects(const char* uuid) {
	Player* p = onlinePlayers[uuid];
	if (playerSign[p]) {
		Json::Value jv = p->getAllEffects();
		if (!jv.isNull()) {
			const char* ret = jv.toStyledString().c_str();
			VA l = strlen(ret);
			std::unique_ptr<char[]> x = std::unique_ptr<char[]>(new char[l + 1]);
			strcpy_s(*(char**)&x, l + 1, ret);
			return *(const char**)&x;
		}
	}
	return NULL;
}

// ��������setPlayerEffects
// ���ܣ������������Ч���б�
// ����������2��
// �������ͣ��ַ������ַ���
// ������⣺uuid - ������ҵ�uuid�ַ�����newEffects - ��Ч���б�json�����ַ���
// ����ֵ���Ƿ����óɹ�
// ����ע���ض������¿��ܲ�������Ϸ��ʵ����Ʒ��
static bool setPlayerEffects(const char* uuid, const char* effs) {
	Player* p = onlinePlayers[uuid];
	if (playerSign[p]) {
		std::string suuid = uuid;
		std::string seffs = effs;
		auto fr = [suuid, seffs]() {
			Player* p = onlinePlayers[suuid];
			if (playerSign[p]) {
				Json::Value jv = toJson(seffs);
				if (jv.isNull()) {
					return;
				}
				p->setAllEffects(jv);
			}
		};
		safeTick(fr);
		return true;
	}
	return false;
}

// ж��bossbar
static bool releaseBossBar(Player* p) {
	if (p) {
		VA t = 0;
		BossEventPacket sec;
		SYMCALL(VA, MSSYM_B1QE12createPacketB1AE16MinecraftPacketsB2AAA2SAB1QA2AVB2QDA6sharedB1UA3ptrB1AA7VPacketB3AAAA3stdB2AAE20W4MinecraftPacketIdsB3AAAA1Z,
			&t, 74);
		if (t) {
			BossEventPacket* pt = (BossEventPacket*)t;
			pt->mEventType = 2;	// remove player
			pt->mHealthPercent = 0;
			pt->mBossID = pt->mPlayerID = *(p->getUniqueID());
			p->sendPacket(t);
			return true;
		}
	}
	return false;
}

// ��������setPlayerBossBar
// ���ܣ���������Զ���Ѫ��
// ����������3��
// �������ͣ��ַ������ַ�����������
// ������⣺uuid - ������ҵ�uuid�ַ�����title - Ѫ�����⣬percent - Ѫ���ٷֱ�
// ����ֵ���Ƿ����óɹ�
static bool setPlayerBossBar(const char* uuid, const char* title, float percent) {
	Player* p = onlinePlayers[uuid];
	if (playerSign[p]) {
		std::string suuid = uuid;
		std::string stitle = GBKToUTF8(title);
		auto fr = [suuid, stitle, percent]() {
			Player* p = onlinePlayers[suuid];
			if (playerSign[p]) {
				if (releaseBossBar(p)) {
					VA t = 0;
					BossEventPacket sec;
					SYMCALL(VA, MSSYM_B1QE12createPacketB1AE16MinecraftPacketsB2AAA2SAB1QA2AVB2QDA6sharedB1UA3ptrB1AA7VPacketB3AAAA3stdB2AAE20W4MinecraftPacketIdsB3AAAA1Z,
						&t, 74);
					if (t) {
						BossEventPacket* pt = (BossEventPacket*)t;
						pt->mName = stitle;
						pt->mEventType = 0;	// add player
						pt->mHealthPercent = percent;
						pt->mBossID = pt->mPlayerID = *(p->getUniqueID());
						p->sendPacket(t);
					}
				}
			}
		};
		safeTick(fr);
		return true;
	}
	return false;
}

// ��������removePlayerBossBar
// ���ܣ��������Զ���Ѫ��
// ����������1��
// �������ͣ��ַ���
// ������⣺uuid - ������ҵ�uuid�ַ���
// ����ֵ���Ƿ�����ɹ�
static bool removePlayerBossBar(const char* uuid) {
	Player* p = onlinePlayers[uuid];
	if (playerSign[p]) {
		std::string suuid = uuid;
		auto fr = [suuid]() {
			Player* p = onlinePlayers[suuid];
			if (playerSign[p]) {
				releaseBossBar(p);
			}
		};
		safeTick(fr);
		return true;
	}
	return false;
}


// ��������teleport
// ���ܣ����������ָ�������ά��
// ����������5��
// �������ͣ��ַ����������ͣ������ͣ������ͣ�����
// ������⣺uuid - ������ҵ�uuid�ַ�����X - x��Y - y��Z - z��dimensionid - ά��ID
// ����ֵ���Ƿ��ͳɹ�
static bool teleport(const char* uuid, float x, float y, float z, int did) {
	Player* p = onlinePlayers[uuid];
	if (playerSign[p]) {
		std::string suuid = uuid;
		auto fr = [suuid, x, y, z, did]() {
			Player* p = onlinePlayers[suuid];
			if (playerSign[p]) {
				Vec3 v;
				v.x = x;
				v.y = y;
				v.z = z;
				p->teleport(&v, did);
			}
		};
		safeTick(fr);
		return true;
	}
	return false;
}


// ��������setPlayerSidebar
// ���ܣ���������Զ���������ʱ�Ʒְ�
// ����������3��
// �������ͣ��ַ������ַ������ַ���
// ������⣺uuid - ������ҵ�uuid�ַ�����title - ��������⣬list - �б��ַ�������
// ����ֵ���Ƿ����óɹ�
// ��ע���б����Ǵӵ�1�п�ʼ���ܼƲ�����15�У�
static bool setPlayerSidebar(const char* uuid, const char* title, const char* list) {
	Player* p = onlinePlayers[uuid];
	if (playerSign[p]) {
		std::string suuid = uuid;
		std::string stitle = GBKToUTF8(title);
		std::string slist = GBKToUTF8(list);
		auto fr = [suuid, stitle, slist]() {
			Player* p = onlinePlayers[suuid];
			if (playerSign[p]) {
				Json::Value jv = toJson(slist);
				std::vector<std::string> l;
				if (!jv.isNull()) {
					for (auto& v : jv) {
						l.push_back(v.asString());
					}
				}
				((Scoreboard*)((Level*)p->getLevel())->getScoreBoard())->sendCustemData(p, stitle, l);
			}
		};
		safeTick(fr);
		return true;
	}
	return false;
}

// ��������removePlayerSidebar
// ���ܣ��������Զ�������
// ����������1��
// �������ͣ��ַ���
// ������⣺uuid - ������ҵ�uuid�ַ���
// ����ֵ���Ƿ�����ɹ�
static bool removePlayerSidebar(const char* uuid) {
	Player* p = onlinePlayers[uuid];
	if (playerSign[p]) {
		std::string suuid = uuid;
		auto fr = [suuid]() {
			Player* p = onlinePlayers[suuid];
			if (playerSign[p]) {
				((Scoreboard*)((Level*)p->getLevel())->getScoreBoard())->removeCustemScoreData(p);
			}
		};
		safeTick(fr);
		return true;
	}
	return false;
}

// ��������getPlayerPermissionAndGametype
// ���ܣ���ȡ���Ȩ������Ϸģʽ
// ����������1��
// �������ͣ��ַ���
// ������⣺uuid - ������ҵ�uuid�ַ���
// ����ֵ��Ȩ����ģʽ��json�ַ���
static const char* getPlayerPermissionAndGametype(const char* uuid) {
	Player* p = onlinePlayers[uuid];
	if (playerSign[p]) {
		mleftlock.lock();
		Json::Value jv;
		jv["oplevel"] = (int)p->getPermission();
		jv["permission"] = (int)p->getPermissionLevel();
		jv["gametype"] = p->getGameType();
		mleftlock.unlock();
		const char* ret = jv.toStyledString().c_str();
		VA l = strlen(ret);
		std::unique_ptr<char[]> x = std::unique_ptr<char[]>(new char[l + 1]);
		strcpy_s(*(char**)&x, l + 1, ret);
		return *(const char**)&x;
	}
	return NULL;
}

// ��������setPlayerPermissionAndGametype
// ���ܣ��������Ȩ������Ϸģʽ
// ����������2��
// �������ͣ��ַ������ַ���
// ������⣺uuid - ������ҵ�uuid�ַ�����newModes - ��Ȩ�޻�ģʽjson�����ַ���
// ����ֵ���Ƿ����óɹ�
// ����ע���ض������¿��ܲ�������Ϸ��ʵ��������
static bool setPlayerPermissionAndGametype(const char* uuid, const char* newModes) {
	Player* p = onlinePlayers[uuid];
	if (playerSign[p]) {
		std::string suuid = uuid;
		std::string jvdata = newModes;
		auto fr = [suuid, jvdata]() {
			Player* p = onlinePlayers[suuid];
			if (playerSign[p]) {
				Json::Value jv = toJson(jvdata);
				if (!jv.isNull()) {
					Json::Value ol = jv["oplevel"];
					Json::Value per = jv["permission"];
					Json::Value ty = jv["gametype"];
					if (!ol.isNull()) {
						p->setPermission((char)ol.asInt());
					}
					if (!per.isNull()) {
						p->setPermissionLevel((char)per.asInt());
						p->setPermission(p->getPermission());
					}
					if (!ty.isNull()) {
						p->setGameType(ty.asInt());
					}
				}
			}
		};
		safeTick(fr);
		return true;
	}
	return false;
}


#endif

// ��������reNameByUuid
// ���ܣ�������һ��ָ���������
// ����������2��
// �������ͣ��ַ������ַ���
// ������⣺uuid - ������ҵ�uuid�ַ�����newName - �µ�����
// ����ֵ���Ƿ������ɹ�
// ����ע���ú������ܲ������ͻ���ʵ����ʾ����
bool reNameByUuid(const char* cuuid, const char* cnewName) {
	bool ret = false;
	std::string uuid = std::string(cuuid);
	std::string newName = GBKToUTF8(cnewName);
	Player* taget = onlinePlayers[uuid];
	if (playerSign[taget]) {
		auto fr = [uuid, newName]() {
			Player* p = onlinePlayers[uuid];
			if (playerSign[p]) {
				p->reName(newName);
			}
		};
		safeTick(fr);
		ret = true;
	}
	return ret;
}

// ��������addPlayerItem
// ���ܣ��������һ����Ʒ
// ����������4��
// �������ͣ��ַ���
// ������⣺uuid - ������ҵ�uuid�ַ�����id - ��Ʒidֵ��aux - ��Ʒ����ֵ��count - ����
// ����ֵ���Ƿ����ӳɹ�
// ����ע���ض������¿��ܲ�������Ϸ��ʵ����Ʒ��
bool addPlayerItem(const char* uuid, int id, short aux, char count) {
	Player* p = onlinePlayers[uuid];
	bool ret = false;
	if (playerSign[p]) {
		std::string suuid = uuid;
		auto fr = [suuid, id, aux, count]() {
			Player* p = onlinePlayers[suuid];
			if (playerSign[p]) {
				ItemStack x;
				x.getFromId(id, aux, count);
				p->addItem((VA)&x);
				p->updateInventory();
			}
		};
		safeTick(fr);
		ret = true;
	}
	return ret;
}


// ��JSON�����и�����һ�����Ϣ
static void addPlayerJsonInfo(Json::Value& jv, Player* p) {
	if (p) {
		jv["playername"] = p->getNameTag();
		int did = p->getDimensionId();
		jv["dimensionid"] = did;
		jv["dimension"] = toDimenStr(did);
		jv["isstand"] = p->isStand();
		jv["XYZ"] = toJson(p->getPos()->toJsonString());
	}
}

// ��������selectPlayer
// ���ܣ���ѯ������һ�����Ϣ
// ����������1��
// �������ͣ��ַ���
// ������⣺uuid - ������ҵ�uuid�ַ���
// ����ֵ����һ�����Ϣjson�ַ���
const char* selectPlayer(const char* uuid) {
	Player* p = onlinePlayers[uuid];
	if (playerSign[p]) {
		mleftlock.lock();
		Json::Value jv;
		addPlayerJsonInfo(jv, p);
		jv["uuid"] = p->getUuid()->toString();
		jv["xuid"] = p->getXuid(p_level);
#if (COMMERCIAL)
		jv["health"] = p->getAttr("health");
#endif
		mleftlock.unlock();
		const char* ret = jv.toStyledString().c_str();
		VA l = strlen(ret);
		std::unique_ptr<char[]> x = std::unique_ptr<char[]>(new char[l + 1]);
		strcpy_s(*(char**)&x, l + 1, ret);
		return *(const char**)&x;
	}
	return NULL;
}

// ��������talkAs
// ���ܣ�ģ����ҷ���һ���ı�
// ����������2��
// �������ͣ��ַ������ַ���
// ������⣺uuid - ������ҵ�uuid�ַ�����msg - ��ģ�ⷢ�͵��ı�
// ����ֵ���Ƿ��ͳɹ�
bool talkAs(const char* uuid, const char* msg) {
	Player* p = onlinePlayers[uuid];
	if (playerSign[p]) {								// IDA ServerNetworkHandler::handle, https://github.com/NiclasOlofsson/MiNET/blob/master/src/MiNET/MiNET/Net/MCPE%20Protocol%20Documentation.md
		std::string suuid = uuid;
		std::string txt = GBKToUTF8(msg);
		auto fr = [suuid, txt]() {
			Player* p = onlinePlayers[suuid];
			if (playerSign[p]) {
				std::string n = p->getNameTag();
				VA nid = p->getNetId();
				VA tpk;
				TextPacket sec;
				SYMCALL(VA, MSSYM_B1QE12createPacketB1AE16MinecraftPacketsB2AAA2SAB1QA2AVB2QDA6sharedB1UA3ptrB1AA7VPacketB3AAAA3stdB2AAE20W4MinecraftPacketIdsB3AAAA1Z,
					&tpk, 9);
				*(char*)(tpk + 40) = 1;
				memcpy((void*)(tpk + 48), &n, sizeof(n));
				memcpy((void*)(tpk + 80), &txt, sizeof(txt));
				SYMCALL(VA, MSSYM_B1QA6handleB1AE20ServerNetworkHandlerB2AAE26UEAAXAEBVNetworkIdentifierB2AAE14AEBVTextPacketB3AAAA1Z,
					p_ServerNetworkHandle, nid, tpk);
			}
		};
		safeTick(fr);
		return true;
	}
	return false;
}

// ��������runcmdAs
// ���ܣ�ģ�����ִ��һ��ָ��
// ����������2��
// �������ͣ��ַ������ַ���
// ������⣺uuid - ������ҵ�uuid�ַ�����cmd - ��ģ��ִ�е�ָ��
// ����ֵ���Ƿ��ͳɹ�
bool runcmdAs(const char* uuid, const char* cmd) {
	Player* p = onlinePlayers[uuid];
	if (playerSign[p]) {
		std::string suuid = uuid;
		std::string scmd = GBKToUTF8(cmd);
		auto fr = [suuid, scmd]() {
			Player* p = onlinePlayers[suuid];
			if (playerSign[p]) {
				VA nid = p->getNetId();
				VA tpk;
				CommandRequestPacket src;
				SYMCALL(VA, MSSYM_B1QE12createPacketB1AE16MinecraftPacketsB2AAA2SAB1QA2AVB2QDA6sharedB1UA3ptrB1AA7VPacketB3AAAA3stdB2AAE20W4MinecraftPacketIdsB3AAAA1Z,
					&tpk, 76);
				memcpy((void*)(tpk + 40), &scmd, sizeof(scmd));
				SYMCALL(VA, MSSYM_B1QA6handleB1AE20ServerNetworkHandlerB2AAE26UEAAXAEBVNetworkIdentifierB2AAE24AEBVCommandRequestPacketB3AAAA1Z,
					p_ServerNetworkHandle, nid, tpk);
			}
		};
		safeTick(fr);
		return true;
	}
	return false;
}



// �ж�ָ���Ƿ�Ϊ����б���ָ��
static bool checkIsPlayer(void* p) {
	return playerSign[(Player*)p];
}

static std::map<unsigned, bool> fids;

// ��ȡһ��δ��ʹ�õĻ���ʱ��������id
static unsigned getFormId() {
	unsigned id = time(0) + rand();
	do {
		++id;
	} while (id == 0 || fids[id]);
	fids[id] = true;
	return id;
}

// ��������releaseForm
// ���ܣ�����һ����
// ����������1��
// �������ͣ�����
// ������⣺formid - ��id
// ����ֵ���Ƿ��ͷųɹ�
//����ע���ѱ����յ��ı��ᱻ�Զ��ͷţ�
bool releaseForm(unsigned fid)
{
	if (fids[fid]) {
		fids.erase(fid);
		return true;
	}
	return false;
}

// ����һ��SimpleForm�ı����ݰ�
static unsigned sendForm(std::string uuid, std::string str)
{
	unsigned fid = getFormId();
	// �˴�����������
	auto fr = [uuid, fid, str]() {
		Player* p = onlinePlayers[uuid];
		if (playerSign[p]) {
			VA tpk;
			ModalFormRequestPacket sec;
			SYMCALL(VA, MSSYM_B1QE12createPacketB1AE16MinecraftPacketsB2AAA2SAB1QA2AVB2QDA6sharedB1UA3ptrB1AA7VPacketB3AAAA3stdB2AAE20W4MinecraftPacketIdsB3AAAA1Z,
				&tpk, 100);
			*(VA*)(tpk + 40) = fid;
			*(std::string*)(tpk + 48) = str;
			p->sendPacket(tpk);
		}
	};
	safeTick(fr);
	return fid;
}

// ��������sendSimpleForm
// ���ܣ���ָ������ҷ���һ���򵥱�
// ����������4��
// �������ͣ��ַ������ַ������ַ������ַ���
// ������⣺uuid - ������ҵ�uuid�ַ�����title - �����⣬content - ���ݣ�buttons - ��ť�ı������ַ���
// ����ֵ�������ı�id��Ϊ 0 ��ʾ����ʧ��
UINT sendSimpleForm(char* uuid, char* title, char* content, char* buttons) {
	Player* p = onlinePlayers[uuid];
	if (!playerSign[p])
		return 0;
	auto stitle = GBKToUTF8(title);
	auto scontent = GBKToUTF8(content);
	auto sbuttons = GBKToUTF8(buttons);
	Json::Value bts;
	Json::Value ja = toJson(sbuttons);
	for (unsigned i = 0; i < ja.size(); i++) {
		Json::Value bt;
		bt["text"] = ja[i];
		bts.append(bt);
	}
	if (bts.isNull())
		bts = toJson("[]");
	std::string str = createSimpleFormString(stitle, scontent, bts);
	return sendForm(uuid, str);
}

// ��������sendModalForm
// ���ܣ���ָ������ҷ���һ��ģʽ�Ի���
// ����������5��
// �������ͣ��ַ������ַ������ַ������ַ������ַ���
// ������⣺uuid - ������ҵ�uuid�ַ�����title - �����⣬content - ���ݣ�button1 ��ť1���⣨����ð�ťselectedΪtrue����button2 ��ť2���⣨����ð�ťselectedΪfalse��
// ����ֵ�������ı�id��Ϊ 0 ��ʾ����ʧ��
UINT sendModalForm(char* uuid, char* title, char* content, char* button1, char* button2) {
	Player* p = onlinePlayers[uuid];
	if (!playerSign[p])
		return 0;
	auto utitle = GBKToUTF8(title);
	auto ucontent = GBKToUTF8(content);
	auto ubutton1 = GBKToUTF8(button1);
	auto ubutton2 = GBKToUTF8(button2);
	std::string str = createModalFormString(utitle, ucontent, ubutton1, ubutton2);
	return sendForm(uuid, str);
}

// ��������sendCustomForm
// ���ܣ���ָ������ҷ���һ���Զ����
// ����������2��
// �������ͣ��ַ������ַ���
// ������⣺uuid - ������ҵ�uuid�ַ�����json - �Զ������json�ַ�����Ҫʹ���Զ�������ͣ��ο�nk��pm��ʽ��minebbsר����
// ����ֵ�������ı�id��Ϊ 0 ��ʾ����ʧ��
UINT sendCustomForm(char* uuid, char* json) {
	Player* p = onlinePlayers[uuid];
	if (!playerSign[p])
		return 0;
	auto ujson = GBKToUTF8(json);
	return sendForm(uuid, ujson);
}

#if (MODULE_05007)

// ��������getscorebroardValue
// ���ܣ���ȡָ�����ָ���Ʒְ��ϵ���ֵ
// ����������2��
// �������ͣ��ַ������ַ���
// ������⣺uuid - ������ҵ�uuid�ַ�����objname - �Ʒְ�Ǽǵ�����
// ����ֵ����ȡ��Ŀ��ֵ����Ŀ�겻��������һ������ָ��
int getscoreboardValue(const char* uuid, const char* objname) {
	Player* p = onlinePlayers[uuid];
	if (!playerSign[p])
		return 0;
	auto oname = GBKToUTF8(objname);
	return getscoreboard(p, oname);
}

#endif

// ���������Ϣ
static void addPlayerInfo(PlayerEvent* pe, Player* p) {
	autoByteCpy(&pe->playername, p->getNameTag().c_str());
	pe->dimensionid = p->getDimensionId();
	autoByteCpy(&pe->dimension, toDimenStr(pe->dimensionid).c_str());
	memcpy(&pe->XYZ, p->getPos(), sizeof(Vec3));
	pe->isstand = p->isStand();
}

static void addPlayerDieInfo(MobDieEvent* ue, Player* pPlayer) {
	autoByteCpy(&ue->playername, pPlayer->getNameTag().c_str());
	ue->dimensionid = pPlayer->getDimensionId();
	autoByteCpy(&ue->dimension, toDimenStr(ue->dimensionid).c_str());
	memcpy(&ue->XYZ, pPlayer->getPos(), sizeof(Vec3));
	ue->isstand = pPlayer->isStand();
}

static void addMobDieInfo(MobDieEvent* ue, Mob* p) {
	autoByteCpy(&ue->mobname, p->getNameTag().c_str());
	int did = p->getDimensionId();
	ue->dimensionid = did;
	memcpy(&ue->XYZ, p->getPos(), sizeof(Vec3));
}

// �ش��˺�Դ��Ϣ
static void getDamageInfo(void* p, void* dsrc, MobDieEvent* ue) {			// IDA Mob::die
	char v72;
	VA  v2[2];
	v2[0] = (VA)p;
	v2[1] = (VA)dsrc;
	auto v7 = *((VA*)(v2[0] + 816));
	auto srActid = (VA*)(*(VA(__fastcall**)(VA, char*))(*(VA*)v2[1] + 64))(
		v2[1], &v72);
	auto SrAct = SYMCALL(Actor*,
		MSSYM_B1QE11fetchEntityB1AA5LevelB2AAE13QEBAPEAVActorB2AAE14UActorUniqueIDB3AAUA1NB1AA1Z,
		v7, *srActid, 0);
	std::string sr_name = "";
	std::string sr_type = "";
	if (SrAct) {
		sr_name = SrAct->getNameTag();
		int srtype = checkIsPlayer(SrAct) ? 319 : SrAct->getEntityTypeId();
		SYMCALL(std::string&, MSSYM_MD5_af48b8a1869a49a3fb9a4c12f48d5a68, &sr_type, srtype);
	}
	Json::Value jv;
	if (checkIsPlayer(p)) {
		addPlayerDieInfo(ue, (Player*)p);
		std::string playertype;				// IDA Player::getEntityTypeId
		SYMCALL(std::string&, MSSYM_MD5_af48b8a1869a49a3fb9a4c12f48d5a68, &playertype, 319);
		autoByteCpy(&ue->mobname, ue->playername);
		autoByteCpy(&ue->mobtype, playertype.c_str());	// "entity.player.name"
	}
	else {
		addMobDieInfo(ue, (Mob*)p);
		autoByteCpy(&ue->mobtype, ((Mob*)p)->getEntityTypeName().c_str());
	}
	autoByteCpy(&ue->srcname, sr_name.c_str());
	autoByteCpy(&ue->srctype, sr_type.c_str());
	ue->dmcase = *((int*)dsrc + 2);
}

//////////////////////////////// ��̬ HOOK ���� ////////////////////////////////

bool hooked = false;

// �˴���ʼ�����첽����
THook2(_CS_MAIN, VA, 0x000B8960,
	VA a1, VA a2, VA a3) {
	initMods();
	return original(a1, a2, a3);
}

// ��ȡָ�����
THook2(_CS_GETSPSCQUEUE, VA, MSSYM_MD5_3b8fb7204bf8294ee636ba7272eec000,
	VA _this) {
	p_spscqueue = original(_this);
	return p_spscqueue;
}

// ��ȡ��Ϸ��ʼ��ʱ������Ϣ
THook2(_CS_ONGAMESESSION, VA,
	MSSYM_MD5_9f3b3524a8d04242c33d9c188831f836,
	void* a1, void* a2, VA* a3, void* a4, void* a5, void* a6, void* a7) {
	p_ServerNetworkHandle = *a3;
	return original(a1, a2, a3, a4, a5, a6, a7);
}

// ��ȡ��ͼ��ʼ����Ϣ
THook2(_CS_LEVELINIT, VA, MSSYM_MD5_96d831b409d1a1984d6ac116f2bd4a55,
	VA a1, VA a2, VA a3, VA a4, VA a5, VA a6, VA a7, VA a8, VA a9, VA a10, VA a11, VA a12, VA a13) {
	VA level = original(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13);
	p_level = level;
	return level;
}

// ע��ָ������������list����ע��
THook2(_CS_ONLISTCMDREG, VA, MSSYM_B1QA5setupB1AE11ListCommandB2AAE22SAXAEAVCommandRegistryB3AAAA1Z,
	VA handle) {
	regHandle = handle;
	for (auto& v : cmddescripts) {
		std::string c = std::string(v.first);
		std::string ct = v.second->description;
		char level = v.second->level;
		char f1 = v.second->flag1;
		char f2 = v.second->flag2;
		SYMCALL(VA, MSSYM_MD5_8574de98358ff66b5a913417f44dd706, handle, &c, ct.c_str(), level, f1, f2);
	}
	return original(handle);
}

// ��������̨����ָ��
THook2(_CS_ONSERVERCMD, bool,
	MSSYM_MD5_b5c9e566146b3136e6fb37f0c080d91e,
	VA _this, std::string* cmd) {
	Events e;
	e.type = EventType::onServerCmd;
	e.mode = ActMode::BEFORE;
	e.result = 0;
	ServerCmdEvent se;
	autoByteCpy(&se.cmd, cmd->c_str());
	e.data = &se;
	bool ret = runCscode(ActEvent.ONSERVERCMD, ActMode::BEFORE, e);
	if (ret) {
		e.result = ret = original(_this, cmd);
		e.mode = ActMode::AFTER;
		runCscode(ActEvent.ONSERVERCMD, ActMode::AFTER, e);
	}
	se.releaseAll();
	return ret;
}

// ��������ָ̨�����
THook2(_CS_ONSERVERCMDOUTPUT, VA,
	MSSYM_MD5_b5f2f0a753fc527db19ac8199ae8f740,
	VA handle, char* str, VA size) {
	if (handle == STD_COUT_HANDLE) {
		Events e;
		e.type = EventType::onServerCmdOutput;
		e.mode = ActMode::BEFORE;
		e.result = 0;
		ServerCmdOutputEvent soe;
		autoByteCpy(&soe.output, str);
		e.data = &soe;
		bool ret = runCscode(ActEvent.ONSERVERCMDOUTPUT, ActMode::BEFORE, e);
		if (ret) {
			VA result = original(handle, str, size);
			e.result = ret;
			e.mode = ActMode::AFTER;
			runCscode(ActEvent.ONSERVERCMDOUTPUT, ActMode::AFTER, e);
			soe.releaseAll();
			return result;
		}
		soe.releaseAll();
		return handle;
	}
	return original(handle, str, size);
}

// ���ѡ���
THook2(_CS_ONFORMSELECT, void,
	MSSYM_MD5_8b7f7560f9f8353e6e9b16449ca999d2,
	VA _this, VA id, VA handle, ModalFormResponsePacket** fp) {
	ModalFormResponsePacket* fmp = *fp;
	Player* p = SYMCALL(Player*, MSSYM_B2QUE15getServerPlayerB1AE20ServerNetworkHandlerB2AAE20AEAAPEAVServerPlayerB2AAE21AEBVNetworkIdentifierB2AAA1EB1AA1Z,
		handle, id, *(char*)((VA)fmp + 16));
	if (p != NULL) {
		UINT fid = fmp->getFormId();
		if (releaseForm(fid)) {
			Events e;
			e.type = EventType::onFormSelect;
			e.mode = ActMode::BEFORE;
			e.result = 0;
			FormSelectEvent fse;
			addPlayerInfo(&fse, p);
			autoByteCpy(&fse.uuid, p->getUuid()->toString().c_str());
			autoByteCpy(&fse.selected, fmp->getSelectStr().c_str());			// �ر���л��sysca11
			fse.formid = fid;
			e.data = &fse;
			bool ret = runCscode(ActEvent.ONFORMSELECT, ActMode::BEFORE, e);
			if (ret) {
				original(_this, id, handle, fp);
				e.result = ret;
				e.mode = ActMode::AFTER;
				runCscode(ActEvent.ONFORMSELECT, ActMode::AFTER, e);
			}
			fse.releaseAll();
			return;
		}
	}
	original(_this, id, handle, fp);
}

// ��Ҳ�����Ʒ
THook2(_CS_ONUSEITEM, bool,
	MSSYM_B1QA9useItemOnB1AA8GameModeB2AAA4UEAAB1UE14NAEAVItemStackB2AAE12AEBVBlockPosB2AAA9EAEBVVec3B2AAA9PEBVBlockB3AAAA1Z,
	void* _this, ItemStack* item, BlockPos* pBlkpos, unsigned __int8 a4, void* v5, Block* pBlk) {
	auto pPlayer = *reinterpret_cast<Player**>(reinterpret_cast<VA>(_this) + 8);
	Events e;
	e.type = EventType::onUseItem;
	e.mode = ActMode::BEFORE;
	e.result = 0;
	UseItemEvent ue;
	addPlayerInfo(&ue, pPlayer);
	memcpy(&ue.position, pBlkpos->getPosition(), sizeof(BPos3));
	autoByteCpy(&ue.itemname, item->getName().c_str());
	ue.itemid = item->getId();
	ue.itemaux = item->getAuxValue();
	e.data = &ue;
	bool ret = runCscode(ActEvent.ONUSEITEM, ActMode::BEFORE, e);
	if (ret) {
		e.result = ret = original(_this, item, pBlkpos, a4, v5, pBlk);
		e.mode = ActMode::AFTER;
		runCscode(ActEvent.ONUSEITEM, ActMode::AFTER, e);
	}
	ue.releaseAll();
	return ret;
}

// ��ҷ��÷���
THook2(_CS_ONPLACEDBLOCK, bool,
	MSSYM_B1QA8mayPlaceB1AE11BlockSourceB2AAA4QEAAB1UE10NAEBVBlockB2AAE12AEBVBlockPosB2AAE10EPEAVActorB3AAUA1NB1AA1Z,
	BlockSource* _this, Block* pBlk, BlockPos* pBlkpos, unsigned __int8 a4, struct Actor* pPlayer, bool _bool) {
	if (pPlayer && checkIsPlayer(pPlayer)) {
		Player* pp = (Player*)pPlayer;
		Events e;
		e.type = EventType::onPlacedBlock;
		e.mode = ActMode::BEFORE;
		e.result = 0;
		PlacedBlockEvent pe;
		addPlayerInfo(&pe, pp);
		pe.blockid = pBlk->getLegacyBlock()->getBlockItemID();
		autoByteCpy(&pe.blockname, pBlk->getLegacyBlock()->getFullName().c_str());
		memcpy(&pe.position, pBlkpos->getPosition(), sizeof(BPos3));
		e.data = &pe;
		bool ret = runCscode(ActEvent.ONPLACEDBLOCK, ActMode::BEFORE, e);
		if (ret) {
			e.result = ret = original(_this, pBlk, pBlkpos, a4, pPlayer, _bool);
			e.mode = ActMode::AFTER;
			runCscode(ActEvent.ONPLACEDBLOCK, ActMode::AFTER, e);
		}
		pe.releaseAll();
		return ret;
	}
	return original(_this, pBlk, pBlkpos, a4, pPlayer, _bool);
}

// ����ƻ�����
THook2(_CS_ONDESTROYBLOCK, bool,
	MSSYM_B2QUE20destroyBlockInternalB1AA8GameModeB2AAA4AEAAB1UE13NAEBVBlockPosB2AAA1EB1AA1Z,
	void* _this, BlockPos* pBlkpos) {
	auto pPlayer = *reinterpret_cast<Player**>(reinterpret_cast<VA>(_this) + 8);
	auto pBlockSource = *(BlockSource**)(*((VA*)_this + 1) + 800);
	auto pBlk = pBlockSource->getBlock(pBlkpos);
	Events e;
	e.type = EventType::onDestroyBlock;
	e.mode = ActMode::BEFORE;
	e.result = 0;
	DestroyBlockEvent de;
	addPlayerInfo(&de, pPlayer);
	de.blockid = pBlk->getLegacyBlock()->getBlockItemID();
	autoByteCpy(&de.blockname, pBlk->getLegacyBlock()->getFullName().c_str());
	memcpy(&de.position, pBlkpos->getPosition(), sizeof(BPos3));
	e.data = &de;
	bool ret = runCscode(ActEvent.ONDESTROYBLOCK, ActMode::BEFORE, e);
	if (ret) {
		e.result = ret = original(_this, pBlkpos);
		e.mode = ActMode::AFTER;
		runCscode(ActEvent.ONDESTROYBLOCK, ActMode::AFTER, e);
	}
	de.releaseAll();
	return ret;
}

// ��ҿ���׼��
THook2(_CS_ONCHESTBLOCKUSE, bool,
	MSSYM_B1QA3useB1AE10ChestBlockB2AAA4UEBAB1UE11NAEAVPlayerB2AAE12AEBVBlockPosB3AAAA1Z,
	void* _this, Player* pPlayer, BlockPos* pBlkpos) {
	auto pBlockSource = (BlockSource*)((Level*)pPlayer->getLevel())->getDimension(pPlayer->getDimensionId())->getBlockSouce();
	auto pBlk = pBlockSource->getBlock(pBlkpos);
	Events e;
	e.type = EventType::onStartOpenChest;
	e.mode = ActMode::BEFORE;
	e.result = 0;
	StartOpenChestEvent de;
	addPlayerInfo(&de, pPlayer);
	de.blockid = pBlk->getLegacyBlock()->getBlockItemID();
	autoByteCpy(&de.blockname, pBlk->getLegacyBlock()->getFullName().c_str());
	memcpy(&de.position, pBlkpos->getPosition(), sizeof(BPos3));
	e.data = &de;
	bool ret = runCscode(ActEvent.ONSTARTOPENCHEST, ActMode::BEFORE, e);
	if (ret) {
		e.result = ret = original(_this, pPlayer, pBlkpos);
		e.mode = ActMode::AFTER;
		runCscode(ActEvent.ONSTARTOPENCHEST, ActMode::AFTER, e);
	}
	de.releaseAll();
	return ret;
}

// ��ҿ�Ͱ׼��
THook2(_CS_ONBARRELBLOCKUSE, bool,
	MSSYM_B1QA3useB1AE11BarrelBlockB2AAA4UEBAB1UE11NAEAVPlayerB2AAE12AEBVBlockPosB3AAAA1Z,
	void* _this, Player* pPlayer, BlockPos* pBlkpos) {
	auto pBlockSource = (BlockSource*)((Level*)pPlayer->getLevel())->getDimension(pPlayer->getDimensionId())->getBlockSouce();
	auto pBlk = pBlockSource->getBlock(pBlkpos);
	Events e;
	e.type = EventType::onStartOpenBarrel;
	e.mode = ActMode::BEFORE;
	e.result = 0;
	StartOpenBarrelEvent de;
	addPlayerInfo(&de, pPlayer);
	de.blockid = pBlk->getLegacyBlock()->getBlockItemID();
	autoByteCpy(&de.blockname, pBlk->getLegacyBlock()->getFullName().c_str());
	memcpy(&de.position, pBlkpos->getPosition(), sizeof(BPos3));
	e.data = &de;
	bool ret = runCscode(ActEvent.ONSTARTOPENBARREL, ActMode::BEFORE, e);
	if (ret) {
		e.result = ret = original(_this, pPlayer, pBlkpos);
		e.mode = ActMode::AFTER;
		runCscode(ActEvent.ONSTARTOPENBARREL, ActMode::AFTER, e);
	}
	de.releaseAll();
	return ret;
}

// ��ҹر�����
THook2(_CS_ONSTOPOPENCHEST, void,
	MSSYM_B1QA8stopOpenB1AE15ChestBlockActorB2AAE15UEAAXAEAVPlayerB3AAAA1Z,
	void* _this, Player* pPlayer) {
	auto real_this = reinterpret_cast<void*>(reinterpret_cast<VA>(_this) - 248);
	auto pBlkpos = reinterpret_cast<BlockActor*>(real_this)->getPosition();
	auto pBlockSource = (BlockSource*)((Level*)pPlayer->getLevel())->getDimension(pPlayer->getDimensionId())->getBlockSouce();
	auto pBlk = pBlockSource->getBlock(pBlkpos);
	Events e;
	e.type = EventType::onStopOpenChest;
	e.mode = ActMode::BEFORE;
	e.result = 0;
	StopOpenChestEvent de;
	addPlayerInfo(&de, pPlayer);
	de.blockid = pBlk->getLegacyBlock()->getBlockItemID();
	autoByteCpy(&de.blockname, pBlk->getLegacyBlock()->getFullName().c_str());
	memcpy(&de.position, pBlkpos->getPosition(), sizeof(BPos3));
	e.data = &de;
	runCscode(ActEvent.ONSTOPOPENCHEST, ActMode::BEFORE, e);
	original(_this, pPlayer);
	e.result = true;
	e.mode = ActMode::AFTER;
	runCscode(ActEvent.ONSTOPOPENCHEST, ActMode::AFTER, e);
	de.releaseAll();
}

// ��ҹر�ľͰ
THook2(_CS_STOPOPENBARREL, void,
	MSSYM_B1QA8stopOpenB1AE16BarrelBlockActorB2AAE15UEAAXAEAVPlayerB3AAAA1Z,
	void* _this, Player* pPlayer) {
	auto real_this = reinterpret_cast<void*>(reinterpret_cast<VA>(_this) - 248);
	auto pBlkpos = reinterpret_cast<BlockActor*>(real_this)->getPosition();
	auto pBlockSource = (BlockSource*)((Level*)pPlayer->getLevel())->getDimension(pPlayer->getDimensionId())->getBlockSouce();
	auto pBlk = pBlockSource->getBlock(pBlkpos);
	Events e;
	e.type = EventType::onStopOpenBarrel;
	e.mode = ActMode::BEFORE;
	e.result = 0;
	StopOpenBarrelEvent de;
	addPlayerInfo(&de, pPlayer);
	de.blockid = pBlk->getLegacyBlock()->getBlockItemID();
	autoByteCpy(&de.blockname, pBlk->getLegacyBlock()->getFullName().c_str());
	memcpy(&de.position, pBlkpos->getPosition(), sizeof(BPos3));
	e.data = &de;
	runCscode(ActEvent.ONSTOPOPENBARREL, ActMode::BEFORE, e);
	original(_this, pPlayer);
	e.result = true;
	e.mode = ActMode::AFTER;
	runCscode(ActEvent.ONSTOPOPENBARREL, ActMode::AFTER, e);
	de.releaseAll();
}

// ��ҷ���ȡ������
THook2(_CS_ONSETSLOT, void,
	MSSYM_B1QE23containerContentChangedB1AE19LevelContainerModelB2AAA6UEAAXHB1AA1Z,
	LevelContainerModel* a1, VA a2) {
	VA v3 = *((VA*)a1 + 26);				// IDA LevelContainerModel::_getContainer
	BlockSource* bs = *(BlockSource**)(*(VA*)(v3 + 808) + 72);
	BlockPos* pBlkpos = (BlockPos*)((char*)a1 + 216);
	Block* pBlk = bs->getBlock(pBlkpos);
	short id = pBlk->getLegacyBlock()->getBlockItemID();
	if (id == 54 || id == 130 || id == 146 || id == -203 || id == 205 || id == 218) {	// �����ӡ�Ͱ��ǱӰ�е������������
		int slot = a2;
		auto v5 = (*(VA(**)(LevelContainerModel*))(*(VA*)a1 + 160))(a1);
		if (v5) {
			ItemStack* pItemStack = (ItemStack*)(*(VA(**)(VA, VA))(*(VA*)v5 + 40))(v5, a2);
			auto nid = pItemStack->getId();
			auto naux = pItemStack->getAuxValue();
			auto nsize = pItemStack->getStackSize();
			auto nname = std::string(pItemStack->getName());
			auto pPlayer = a1->getPlayer();
			Events e;
			e.type = EventType::onSetSlot;
			e.mode = ActMode::BEFORE;
			e.result = 0;
			SetSlotEvent de;
			addPlayerInfo(&de, pPlayer);
			de.itemid = nid;
			de.itemcount = nsize;
			autoByteCpy(&de.itemname, nname.c_str());
			de.itemaux = naux;
			memcpy(&de.position, pBlkpos, sizeof(BPos3));
			de.blockid = id;
			autoByteCpy(&de.blockname, pBlk->getLegacyBlock()->getFullName().c_str());
			de.slot = slot;
			e.data = &de;
			bool ret = runCscode(ActEvent.ONSETSLOT, ActMode::BEFORE, e);
			if (ret) {
				original(a1, a2);
				e.result = true;
				e.mode = ActMode::AFTER;
				runCscode(ActEvent.ONSETSLOT, ActMode::AFTER, e);
			}
			de.releaseAll();
		}
		else
			original(a1, a2);
	}
	else
		original(a1, a2);
}

// ����л�ά��
THook2(_CS_ONCHANGEDIMENSION, bool,
	MSSYM_B2QUE21playerChangeDimensionB1AA5LevelB2AAA4AEAAB1UE11NPEAVPlayerB2AAE26AEAVChangeDimensionRequestB3AAAA1Z,
	void* _this, Player* pPlayer, void* req) {
	Events e;
	e.type = EventType::onChangeDimension;
	e.mode = ActMode::BEFORE;
	e.result = 0;
	ChangeDimensionEvent de;
	addPlayerInfo(&de, pPlayer);
	e.data = &de;
	bool ret = runCscode(ActEvent.ONCHANGEDIMENSION, ActMode::BEFORE, e);
	if (ret) {
		e.result = ret = original(_this, pPlayer, req);
		e.mode = ActMode::AFTER;
		runCscode(ActEvent.ONCHANGEDIMENSION, ActMode::AFTER, e);
	}
	de.releaseAll();
	return ret;
}

// ��������
THook2(_CS_ONMOBDIE, void,
	MSSYM_B1QA3dieB1AA3MobB2AAE26UEAAXAEBVActorDamageSourceB3AAAA1Z,
	Mob* _this, void* dmsg) {
	Events e;
	e.type = EventType::onMobDie;
	e.mode = ActMode::BEFORE;
	e.result = 0;
	MobDieEvent de;
	getDamageInfo(_this, dmsg, &de);
	e.data = &de;
	bool ret = runCscode(ActEvent.ONMOBDIE, ActMode::BEFORE, e);
	if (ret) {
		original(_this, dmsg);
		e.result = ret;
		e.mode = ActMode::AFTER;
		runCscode(ActEvent.ONMOBDIE, ActMode::AFTER, e);
	}
	de.releaseAll();
}

// �������
THook2(_CS_PLAYERRESPAWN, void, MSSYM_B1QA7respawnB1AA6PlayerB2AAA7UEAAXXZ,
	Player* pPlayer) {
	Events e;
	e.type = EventType::onRespawn;
	e.mode = ActMode::BEFORE;
	e.result = 0;
	RespawnEvent de;
	addPlayerInfo(&de, pPlayer);
	e.data = &de;
	bool ret = runCscode(ActEvent.ONRESPAWN, ActMode::BEFORE, e);
	if (ret) {
		original(pPlayer);
		e.result = ret;
		e.mode = ActMode::AFTER;
		runCscode(ActEvent.ONRESPAWN, ActMode::AFTER, e);
	}
	de.releaseAll();
}

// ������Ϣ
THook2(_CS_ONCHAT, void,
	MSSYM_MD5_ad251f2fd8c27eb22c0c01209e8df83c,
	void* _this, std::string& player_name, std::string& target, std::string& msg, std::string& chat_style) {
	Events e;
	e.type = EventType::onChat;
	e.mode = ActMode::BEFORE;
	e.result = 0;
	ChatEvent de;
	autoByteCpy(&de.playername, player_name.c_str());
	autoByteCpy(&de.target, target.c_str());
	autoByteCpy(&de.msg, msg.c_str());
	autoByteCpy(&de.chatstyle, chat_style.c_str());
	e.data = &de;
	bool ret = runCscode(ActEvent.ONCHAT, ActMode::BEFORE, e);
	if (ret) {
		original(_this, player_name, target, msg, chat_style);
		e.result = ret;
		e.mode = ActMode::AFTER;
		runCscode(ActEvent.ONCHAT, ActMode::AFTER, e);
	}
	de.releaseAll();
}

// �����ı�
THook2(_CS_ONINPUTTEXT, void,
	MSSYM_B1QA6handleB1AE20ServerNetworkHandlerB2AAE26UEAAXAEBVNetworkIdentifierB2AAE14AEBVTextPacketB3AAAA1Z,
	VA _this, VA id, TextPacket* tp) {
	Player* p = SYMCALL(Player*, MSSYM_B2QUE15getServerPlayerB1AE20ServerNetworkHandlerB2AAE20AEAAPEAVServerPlayerB2AAE21AEBVNetworkIdentifierB2AAA1EB1AA1Z,
		_this, id, *((char*)tp + 16));
	Events e;
	e.type = EventType::onInputText;
	e.mode = ActMode::BEFORE;
	e.result = 0;
	InputTextEvent de;
	if (p != NULL) {
		addPlayerInfo(&de, p);
	}
	autoByteCpy(&de.msg, tp->toString().c_str());
	e.data = &de;
	bool ret = runCscode(ActEvent.ONINPUTTEXT, ActMode::BEFORE, e);
	if (ret) {
		original(_this, id, tp);
		e.result = ret;
		e.mode = ActMode::AFTER;
		runCscode(ActEvent.ONINPUTTEXT, ActMode::AFTER, e);
	}
	de.releaseAll();
}

// MakeWP
static Player* MakeWP(CommandOrigin& ori) {
	if (ori.getOriginType() == OriginType::Player) {
		return (Player*)ori.getEntity();
	}
	return 0;
}

// ���ִ��ָ��
THook2(_CS_ONINPUTCOMMAND, VA,
	MSSYM_B1QE14executeCommandB1AE17MinecraftCommandsB2AAA4QEBAB1QE10AUMCRESULTB2AAA1VB2QDA6sharedB1UA3ptrB1AE15VCommandContextB3AAAA3stdB3AAUA1NB1AA1Z,
	VA _this, VA mret, std::shared_ptr<CommandContext> x, char a4) {
	Player* p = MakeWP(x->getOrigin());
	if (p) {
		Events e;
		e.type = EventType::onInputCommand;
		e.mode = ActMode::BEFORE;
		e.result = 0;
		InputCommandEvent de;
		if (p != NULL) {
			addPlayerInfo(&de, p);
		}
		autoByteCpy(&de.cmd, x->getCmd().c_str());
		VA mcmd = 0;
		e.data = &de;
		bool ret = runCscode(ActEvent.ONINPUTCOMMAND, ActMode::BEFORE, e);
		if (ret) {
			mcmd = original(_this, mret, x, a4);
			e.result = ret;
			e.mode = ActMode::AFTER;
			runCscode(ActEvent.ONINPUTCOMMAND, ActMode::AFTER, e);
		}
		de.releaseAll();
		return mcmd;
	}
	return original(_this, mret, x, a4);
}

// ��Ҽ�������
THook2(_CS_ONCREATEPLAYER, VA,
	MSSYM_B1QE14onPlayerJoinedB1AE16ServerScoreboardB2AAE15UEAAXAEBVPlayerB3AAAA1Z,
	VA a1, Player* pPlayer) {
	VA hret = original(a1, pPlayer);
	Events e;
	e.type = EventType::onLoadName;
	e.mode = ActMode::BEFORE;
	e.result = 0;
	LoadNameEvent le;
	autoByteCpy(&le.playername, pPlayer->getNameTag().c_str());
	auto uuid = pPlayer->getUuid()->toString();
	autoByteCpy(&le.uuid, uuid.c_str());
	autoByteCpy(&le.xuid, pPlayer->getXuid(p_level).c_str());
#if (COMMERCIAL)
	autoByteCpy(&le.ability, getAbilities(pPlayer).toStyledString().c_str());
#endif
	e.data = &le;
	onlinePlayers[uuid] = pPlayer;
	playerSign[pPlayer] = true;
	bool ret = runCscode(ActEvent.ONLOADNAME, ActMode::BEFORE, e);
	if (ret) {
		e.result = ret;
		e.mode = ActMode::AFTER;
		runCscode(ActEvent.ONLOADNAME, ActMode::AFTER, e);
	}
	le.releaseAll();
	return hret;
}

// ����뿪��Ϸ
THook2(_CS_ONPLAYERLEFT, void,
	MSSYM_B2QUE12onPlayerLeftB1AE20ServerNetworkHandlerB2AAE21AEAAXPEAVServerPlayerB3AAUA1NB1AA1Z,
	VA _this, Player* pPlayer, char v3) {
	Events e;
	e.type = EventType::onPlayerLeft;
	e.mode = ActMode::BEFORE;
	e.result = 0;
	PlayerLeftEvent le;
	autoByteCpy(&le.playername, pPlayer->getNameTag().c_str());
	auto uuid = pPlayer->getUuid()->toString();
	autoByteCpy(&le.uuid, uuid.c_str());
	autoByteCpy(&le.xuid, pPlayer->getXuid(p_level).c_str());
#if (COMMERCIAL)
	autoByteCpy(&le.ability, getAbilities(pPlayer).toStyledString().c_str());
#endif
	e.data = &le;
	bool ret = runCscode(ActEvent.ONPLAYERLEFT, ActMode::BEFORE, e);
	playerSign[pPlayer] = false;
	playerSign.erase(pPlayer);
	onlinePlayers[uuid] = NULL;
	onlinePlayers.erase(uuid);
	if (ret) {
		original(_this, pPlayer, v3);
		e.result = ret;
		e.mode = ActMode::AFTER;
		runCscode(ActEvent.ONPLAYERLEFT, ActMode::AFTER, e);
	}
	le.releaseAll();
}



// �˴�Ϊ��ֹ����������֣����ó���
THook2(_CS_ONLOGOUT, VA,
	MSSYM_B3QQUE13EServerPlayerB2AAA9UEAAPEAXIB1AA1Z,
	Player* a1, VA a2) {
	mleftlock.lock();
	if (playerSign[a1]) {				// �������ǳ���Ϸ�û���ִ��ע��
		playerSign[a1] = false;
		playerSign.erase(a1);
		const std::string* uuid = NULL;
		for (auto& p : onlinePlayers) {
			if (p.second == a1) {
				uuid = &p.first;
				break;
			}
		}
		if (uuid)
			onlinePlayers.erase(*uuid);
	}
	mleftlock.unlock();
	return original(a1, a2);
}

// ����ƶ���Ϣ����
THook2(_CS_ONMOVE, VA,
	MSSYM_B2QQE170MovePlayerPacketB2AAA4QEAAB1AE10AEAVPlayerB2AAE14W4PositionModeB1AA11B1AA2HHB1AA1Z,
	void* _this, Player* pPlayer, char v3, int v4, int v5) {
	int reg = (beforecallbacks[ActEvent.ONMOVE] != NULL ? beforecallbacks[ActEvent.ONMOVE]->size() : 0) +
		(aftercallbacks[ActEvent.ONMOVE] != NULL ? beforecallbacks[ActEvent.ONMOVE]->size() : 0);
	if (!reg)
		return original(_this, pPlayer, v3, v4, v5);
	VA reto = 0;
	Events e;
	e.type = EventType::onMove;
	e.mode = ActMode::BEFORE;
	e.result = 0;
	MoveEvent de;
	addPlayerInfo(&de, pPlayer);
	e.data = &de;
	bool ret = runCscode(ActEvent.ONMOVE, ActMode::BEFORE, e);
	if (ret) {
		reto = original(_this, pPlayer, v3, v3, v4);
		e.result = ret;
		runCscode(ActEvent.ONMOVE, ActMode::AFTER, e);
	}
	de.releaseAll();
	return reto;
}

// ��ҹ�������
THook2(_CS_ONATTACK, bool,
	MSSYM_B1QA6attackB1AA6PlayerB2AAA4UEAAB1UE10NAEAVActorB3AAAA1Z,
	Player* pPlayer, Actor* pa) {
	Events e;
	e.type = EventType::onAttack;
	e.mode = ActMode::BEFORE;
	e.result = 0;
	AttackEvent de;
	addPlayerInfo(&de, pPlayer);
	memcpy(&de.actorpos, pa->getPos(), sizeof(Vec3));
	autoByteCpy(&de.actorname, pa->getNameTag().c_str());
	autoByteCpy(&de.actortype, pa->getTypeName().c_str());
	e.data = &de;
	bool ret = runCscode(ActEvent.ONATTACK, ActMode::BEFORE, e);
	if (ret) {
		e.result = ret = original(pPlayer, pa);
		e.mode = ActMode::AFTER;
		runCscode(ActEvent.ONATTACK, ActMode::AFTER, e);
	}
	de.releaseAll();
	return ret;
}

// ȫͼ��Χ��ը����
THook2(_CS_ONLEVELEXPLODE, void,
	MSSYM_B1QA7explodeB1AA5LevelB2AAE20QEAAXAEAVBlockSourceB2AAA9PEAVActorB2AAA8AEBVVec3B2AAA1MB1UA4N3M3B1AA1Z,
	VA _this, BlockSource* a2, Actor* a3, Vec3* a4, float a5, bool a6, bool a7, float a8, bool a9) {
	Events e;
	e.type = EventType::onLevelExplode;
	e.mode = ActMode::BEFORE;
	e.result = 0;
	LevelExplodeEvent de;
	memcpy(&de.position, a4, sizeof(Vec3));
	int did = a2 ? a2->getDimensionId() : -1;
	de.dimensionid = did;
	autoByteCpy(&de.dimension, toDimenStr(did).c_str());
	if (a3) {
		autoByteCpy(&de.entity, a3->getEntityTypeName().c_str());
		de.entityid = a3->getEntityTypeId();
		int i = a3->getDimensionId();
		de.dimensionid = i;
		autoByteCpy(&de.dimension, toDimenStr(i).c_str());
	}
	de.explodepower = a5;
	e.data = &de;
	bool ret = runCscode(ActEvent.ONLEVELEXPLODE, ActMode::BEFORE, e);
	if (ret) {
		original(_this, a2, a3, a4, a5, a6, a7, a8, a9);
		e.result = ret;
		e.mode = ActMode::AFTER;
		runCscode(ActEvent.ONLEVELEXPLODE, ActMode::AFTER, e);
	}
	de.releaseAll();
}
// ����ê��ը����
THook2(_CS_SETRESPWNEXPLOREDE, bool,
	MSSYM_B1QE11trySetSpawnB1AE18RespawnAnchorBlockB2AAA2CAB1UE11NAEAVPlayerB2AAE12AEBVBlockPosB2AAE15AEAVBlockSourceB2AAA9AEAVLevelB3AAAA1Z,
	Player* pPlayer, BlockPos* a2, BlockSource* a3, Level* a4) {
	auto v8 = a3->getBlock(a2);
	auto v9 = (VA*)*((VA*)v8 + 2);
	VA qwt = SYM_OBJECT(VA, 0x191E308);
	if (!*(char*)(*v9 + 32 * qwt + 460)
		|| !(((unsigned int)*((unsigned __int16*)v8 + 4) >> (*(char*)(*v9 + 32 * qwt + 444)
			- *(char*)(*v9 + 32 * qwt + 448)
			+ 1)) & (0xFFFF >> (*(char*)(*v9 + 32 * qwt + 440)
				- *(char*)(*v9 + 32 * qwt + 448)))))
	{
		return original(pPlayer, a2, a3, a4);
	}
	struct VA_tmp { VA v; };
	if (a3->getDimensionId() != 1) {
		if (!*(char*)(*((VA*)pPlayer + 102) + 7664)) {
			float pw = SYM_OBJECT(float, MSSYM_B2UUA4realB1AA840a00000);
			if (!*(char*)&(((VA_tmp*)a4)[958].v)) {
				if (pw != 0.0) {
					std::string blkname = v8->getLegacyBlock()->getFullName();
					Events e;
					e.type = EventType::onLevelExplode;
					e.mode = ActMode::BEFORE;
					e.result = 0;
					LevelExplodeEvent de;
					de.blockid = v8->getLegacyBlock()->getBlockItemID();
					autoByteCpy(&de.blockname, v8->getLegacyBlock()->getFullName().c_str());
					de.dimensionid = a3->getDimensionId();
					autoByteCpy(&de.dimension, toDimenStr(de.dimensionid).c_str());
					de.position.x = a2->getPosition()->x;
					de.position.y = a2->getPosition()->y;
					de.position.z = a2->getPosition()->z;
					de.explodepower = pw;
					e.data = &de;
					bool ret = runCscode(ActEvent.ONLEVELEXPLODE, ActMode::BEFORE, e);
					if (ret) {
						ret = original(pPlayer, a2, a3, a4);
						e.result = ret;
						e.mode = ActMode::AFTER;
						runCscode(ActEvent.ONLEVELEXPLODE, ActMode::AFTER, e);
					}
					de.releaseAll();
					return ret;
				}
			}
		}
	}
	return original(pPlayer, a2, a3, a4);
}

static char localpath[MAX_PATH] = { 0 };

// ��ȡBDS��������·��
static std::string getLocalPath() {
	if (!localpath[0]) {
		GetModuleFileNameA(NULL, localpath, _countof(localpath));
		for (VA l = strlen(localpath); l >= 0; l--) {
			if (localpath[l] == '\\') {
				localpath[l] = localpath[l + 1] = localpath[l + 2] = 0;
				break;
			}
		}
	}
	return std::string(localpath);
}

static bool inited = false;
static VA attcount = 0;

static ICLRMetaHost* pMetaHost = nullptr;
static ICLRMetaHostPolicy* pMetaHostPolicy = nullptr;
static ICLRRuntimeHost* pRuntimeHost = nullptr;
static ICLRRuntimeInfo* pRuntimeInfo = nullptr;

// ��ʼ��.Net����
static void initNetFramework() {
	SetCurrentDirectoryA(getLocalPath().c_str());
	DWORD dwRet = 0;
	//wchar_t curDir[256] = { 0 };
	HRESULT hr = CLRCreateInstance(CLSID_CLRMetaHost, IID_ICLRMetaHost, (LPVOID*)&pMetaHost);
	hr = pMetaHost->GetRuntime(L"v4.0.30319", IID_PPV_ARGS(&pRuntimeInfo));
	if (!FAILED(hr))
	{
		hr = pRuntimeInfo->GetInterface(CLSID_CLRRuntimeHost, IID_PPV_ARGS(&pRuntimeHost));
		if (FAILED(hr))
			return;
		hr = pRuntimeHost->Start();
		if (FAILED(hr))
			return;
		std::wstring curDllandVer = GetDllPathandVersion();	// ��ȡƽ̨·���Ͱ汾�����ŷָ�
		// �ֲ���������������csr���
		{
			std::string path = "CSR";	// �̶�Ŀ¼ - CSR
			std::string pair = path + "\\*.csr.dll";
			WIN32_FIND_DATAA ffd;
			HANDLE dfh = FindFirstFileA(pair.c_str(), &ffd);
			if (INVALID_HANDLE_VALUE != dfh) {
				do
				{
					if (!(ffd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY))
					{
						std::string fileName = std::string(path + "\\" + ffd.cFileName);
						int len = MultiByteToWideChar(CP_ACP, 0, fileName.c_str(), fileName.length(), NULL, 0);
						LPWSTR w_str = new WCHAR[len + 1];
						w_str[len] = L'\0';
						MultiByteToWideChar(CP_ACP, 0, fileName.c_str(), fileName.length(), w_str, len);
						LPCWSTR dllName = w_str;
						std::wcout << L"[CSR] load " << dllName << std::endl;
						hr = pRuntimeHost->ExecuteInDefaultAppDomain(dllName,	// ���ʵ��·��
							L"CSR.Plugin",										// ���ͨ������
							L"onServerStart",									// ���ͨ�ó�ʼ���ӿ�
							curDllandVer.c_str(),								// �ش�lib�Ͱ汾��
							&dwRet);
						if (FAILED(hr)) {
							wprintf(L"[File] %s api load failed.\n", dllName);
							continue;
						}
					}
				} while (FindNextFileA(dfh, &ffd) != 0);
				FindClose(dfh);
			}
		}
	}
	else {
		releaseNetFramework();
	}
}

// ж��.Net����
static void releaseNetFramework() {
	beforecallbacks.clear();
	aftercallbacks.clear();
	if (pRuntimeHost != nullptr)
		pRuntimeHost->Stop();
	if (pRuntimeInfo != nullptr)
	{
		pRuntimeInfo->Release();
		pRuntimeInfo = nullptr;
	}
	if (pRuntimeHost != nullptr)
	{
		pRuntimeHost->Release();
		pRuntimeHost = nullptr;
	}
	if (pMetaHost != nullptr)
	{
		pMetaHost->Release();
		pMetaHost = nullptr;
	}
}

// ���ӵ��ú���
static std::unordered_map<std::string, void*> extraApi;

static void initExtraApi() {
#if (COMMERCIAL)
	extraApi["getStructure"] = &getStructure;
	extraApi["setStructure"] = &setStructure;
	extraApi["getPlayerAbilities"] = &getPlayerAbilities;
	extraApi["setPlayerAbilities"] = &setPlayerAbilities;
	extraApi["getPlayerAttributes"] = &getPlayerAttributes;
	extraApi["setPlayerTempAttributes"] = &setPlayerTempAttributes;
	extraApi["getPlayerMaxAttributes"] = &getPlayerMaxAttributes;
	extraApi["setPlayerMaxAttributes"] = &setPlayerMaxAttributes;
	extraApi["getPlayerItems"] = &getPlayerItems;
	extraApi["setPlayerItems"] = &setPlayerItems;
	extraApi["getPlayerSelectedItem"] = &getPlayerSelectedItem;
	extraApi["addPlayerItemEx"] = &addPlayerItemEx;
	extraApi["addPlayerItem"] = &addPlayerItem;
	extraApi["getPlayerEffects"] = &getPlayerEffects;
	extraApi["setPlayerEffects"] = &setPlayerEffects;
	extraApi["setPlayerBossBar"] = &setPlayerBossBar;
	extraApi["removePlayerBossBar"] = &removePlayerBossBar;
	extraApi["transferserver"] = &transferserver;
	extraApi["teleport"] = &teleport;
	extraApi["setPlayerSidebar"] = &setPlayerSidebar;
	extraApi["removePlayerSidebar"] = &removePlayerSidebar;
	extraApi["getPlayerPermissionAndGametype"] = &getPlayerPermissionAndGametype;
	extraApi["setPlayerPermissionAndGametype"] = &setPlayerPermissionAndGametype;
#endif
}

void* getExtraAPI(const char* apiname) {
	std::string k = std::string(apiname);
	return extraApi[k];
}

// ��ʼ������
static void initMods()
{
	if (inited) {
		return;
	}
	inited = true;
	initNetFramework();
}

// �ײ����

static std::unordered_map<void*, void**> hooks;

void** getOriginalData(void* hook) {
	return hooks[hook];
}

HookErrorCode mTHook2(RVA sym, void* hook) {
	hooks[hook] = new void* [1]{ 0 };
	void** org = hooks[hook];
	*org = ((char*)GetModuleHandle(NULL)) + sym;
	return Hook<void*>(org, hook);
}

unsigned long long dlsym(int rva) {
	return (VA)GetModuleHandle(NULL) + rva;
}

bool cshook(int rva, void* hook, void** org) {
	*org = ((char*)GetModuleHandle(NULL)) + rva;
	return Hook<void*>(org, hook) == HookErrorCode::ERR_SUCCESS;
}

bool csunhook(void* hook, void** org) {
	return UnHook<void*>(org, hook) == HookErrorCode::ERR_SUCCESS;
}


void init() {
#if (!COMMERCIAL)
	std::cout << u8"{[���] Net�������ƽ̨(������)��װ�ء���ƽ̨����LGPLЭ�鷢�С�" << std::endl;
#else
	std::cout << u8"{[���] Net�������ƽ̨��װ�ء�" << std::endl;
#endif
	std::wcout << L"version=" << VERSION << std::endl;
}

void exit() {
	releaseNetFramework();
}
