#include "scoreboard.hpp"
#include "../mod.h"

#if (MODULE_05007)

static Scoreboard* scoreboard;//����Ʒְ�����

//��ȡ�����ָ���Ʒְ��µ����ݣ�����������������Ʒְ��������ҵ�ֵ��
int getscoreboard(Player* p,std::string objname) {
	auto testobj = scoreboard->getobject(&objname);
	if (!testobj) {
		std::cout << u8"û���ҵ���Ӧ�Ʒְ壬�Զ�����: " << objname << std::endl;
		std::string cmd = "scoreboard objectives add \"" + objname + "\" dummy money";
		runcmd(cmd.c_str());
		return 0;
	}

	__int64 a2[2];
	auto scoreid = scoreboard->getScoreboardID(p);
	auto scores = testobj->getplayerscoreinfo((ScoreInfo*)a2, scoreid);

	return (__int64)scores->getcount();
}

// �Ʒְ�����ע�ᣨ����ʱ��ȡ���еļƷְ����ƣ�
THook(void*, MSSYM_B2QQE170ServerScoreboardB2AAA4QEAAB1AE24VCommandSoftEnumRegistryB2AAE16PEAVLevelStorageB3AAAA1Z, void* _this, void* a2, void* a3) {
	scoreboard = (Scoreboard*)original(_this, a2, a3);
	return scoreboard;
}

#endif
