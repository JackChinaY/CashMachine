//// PBC.cpp : 定义控制台应用程序的入口点。
////
//
#include "stdafx.h"
#include "Debug\include\pbc.h"
#include "Debug\include\pbc_test.h"
#include <iostream>

//int main(int argc, char **argv)
//{
//
//	pairing_t pairing;
//	pbc_demo_pairing_init(pairing, argc, argv);
//	if (!pairing_is_symmetric(pairing)) pbc_die("pairing must be symmetric");
//	element_t P, Ppub, x, S, H, t1, t2, t3, t4;
//	element_init_Zr(x, pairing);
//	element_init_Zr(H, pairing);
//	element_init_Zr(t1, pairing);
//
//	element_init_G1(S, pairing);
//	element_init_G1(P, pairing);
//	element_init_G1(Ppub, pairing);
//	element_init_G1(t2, pairing);
//
//	element_init_GT(t3, pairing);
//	element_init_GT(t4, pairing);
//
//	printf("ZSS short signature schema\n");
//	printf("KEYGEN\n");
//	element_random(x);
//	element_random(P);
//	element_mul_zn(Ppub, P, x);
//	element_printf("P = %B\n", P);
//	element_printf("x = %B\n", x);
//	element_printf("Ppub = %B\n", Ppub);
//
//	printf("SIGN\n");
//	element_from_hash(H, "Message", 7);
//	element_add(t1, H, x);
//	element_invert(t1, t1);
//	element_mul_zn(S, P, t1);
//	printf("Signature of message \"Message\" is:\n");
//	element_printf("S = %B\n", S);
//
//	printf("VERIFY\n");
//	element_from_hash(H, "Message", 7);
//	element_mul_zn(t2, P, H);
//	element_add(t2, t2, Ppub);
//	element_pairing(t3, t2, S);
//	element_pairing(t4, P, P);
//	element_printf("e(H(m)P + Ppub, S) = %B\n", t3);
//	element_printf("e(P, P) = %B\n", t4);
//	if (!element_cmp(t3, t4)) printf("Signature is valid\n");
//	else printf("Signature is invalid\n");
//	element_clear(P);
//	element_clear(Ppub);
//	element_clear(x);
//	element_clear(S);
//	element_clear(H);
//	element_clear(t1);
//	element_clear(t2);
//	element_clear(t3);
//	element_clear(t4);
//	pairing_clear(pairing);
//
//	printf("Have a good day!\n");
//	std::cin.get();
//	return 0;
//}


//#include "Debug\include\pbc.h"
//#include "Debug\include\pbc_test.h"
//
#define LEN 6
int main(int argc, char **argv) {
	pairing_t pairing;
	element_t s, x, r;
	element_t P, Ppub, Qu, Du, Su, Xu, Yu, V;
	element_t T1, T2;
	double time1, time2;
	int byte;
	pbc_demo_pairing_init(pairing, argc, argv);
	//将变量初始化为Zr上的元素
	element_init_Zr(s, pairing);
	element_init_Zr(r, pairing);
	element_init_Zr(x, pairing);
	//将变量初始化为G1上的元素
	element_init_G1(P, pairing);
	element_init_G1(Ppub, pairing);
	element_init_G1(Qu, pairing);
	element_init_G1(Du, pairing);
	element_init_G1(Su, pairing);
	element_init_G1(Xu, pairing);
	element_init_G1(Yu, pairing);
	element_init_G1(V, pairing);
	//将变量初始化为GT中的元素
	element_init_GT(T1, pairing);
	element_init_GT(T2, pairing);
	//判断所用的配对是否为对称配对
	if (!pairing_is_symmetric(pairing)) {
		fprintf(stderr, "只能在对称配对下运行");
		exit(1);
	}
	printf("BasicCL-PKE scheme\n");
	printf("———————————系统建立阶段——————————\n");
	element_random(s);
	element_random(P);
	element_mul_zn(Ppub, P, s);
	element_printf("P=%B\n", P);
	element_printf("s=%B\n", s);
	element_printf("Ppub=%B\n", Ppub);
	printf("———————部分私钥提取———————\n");
	element_random(Qu);//随机选取Qu
	element_mul_zn(Du, Qu, s);//Du=sQu
	element_printf("private key is Du=%B\n", Du);
	printf("—————设置秘密值阶段————\n");
	element_random(x);
	element_printf("秘密值为=%B\n", x);
	printf("—————设置私钥————\n");
	element_mul_zn(Su, Du, x);
	element_printf("完全私钥 Su=%B\n", Su);
	printf("—————设置公钥————\n");
	element_mul_zn(Xu, P, x);//Xu=xP
	element_mul_zn(Yu, Ppub, x);//Yu=xP
	printf("公钥为：\n");
	element_printf("Xu=%B\n", Xu);
	element_printf("Yu=%B\n", Yu);
	printf("———————————加密阶段——————————\n");
	pairing_apply(T1, Xu, Ppub, pairing);//T1=e(Xu,Ppub)
	pairing_apply(T2, Yu, P, pairing);//T2=e(Yu,P)
									  //判断公钥是否正确
	if (!element_cmp(T1, T2)) {
		element_random(r);
		element_mul_zn(V, P, r);//V=rP注意顺序
		pairing_apply(T1, Yu, Qu, pairing);//T1=e(Yu,Qu)
		element_pow_zn(T1, T1, r);//T1^r
		element_printf("V=%B\n", V);
		element_printf("e(Yu,Qu)^r=%B\n", T1);
	}
	else
	{
		printf("错误！ 公钥不正确\n");
		exit(1);
	}
	printf("———————————解密阶段——————————\n");
	pairing_apply(T2, V, Su, pairing);
	element_printf("e(V,Su)=%B\n", T2);
	byte = element_length_in_bytes(V);
	printf("密文总共字节长度为%d\n", byte + 128);
	return 0;
}