//// PBC.cpp : �������̨Ӧ�ó������ڵ㡣
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
	//��������ʼ��ΪZr�ϵ�Ԫ��
	element_init_Zr(s, pairing);
	element_init_Zr(r, pairing);
	element_init_Zr(x, pairing);
	//��������ʼ��ΪG1�ϵ�Ԫ��
	element_init_G1(P, pairing);
	element_init_G1(Ppub, pairing);
	element_init_G1(Qu, pairing);
	element_init_G1(Du, pairing);
	element_init_G1(Su, pairing);
	element_init_G1(Xu, pairing);
	element_init_G1(Yu, pairing);
	element_init_G1(V, pairing);
	//��������ʼ��ΪGT�е�Ԫ��
	element_init_GT(T1, pairing);
	element_init_GT(T2, pairing);
	//�ж����õ�����Ƿ�Ϊ�Գ����
	if (!pairing_is_symmetric(pairing)) {
		fprintf(stderr, "ֻ���ڶԳ����������");
		exit(1);
	}
	printf("BasicCL-PKE scheme\n");
	printf("����������������������ϵͳ�����׶Ρ�������������������\n");
	element_random(s);
	element_random(P);
	element_mul_zn(Ppub, P, s);
	element_printf("P=%B\n", P);
	element_printf("s=%B\n", s);
	element_printf("Ppub=%B\n", Ppub);
	printf("������������������˽Կ��ȡ��������������\n");
	element_random(Qu);//���ѡȡQu
	element_mul_zn(Du, Qu, s);//Du=sQu
	element_printf("private key is Du=%B\n", Du);
	printf("������������������ֵ�׶Ρ�������\n");
	element_random(x);
	element_printf("����ֵΪ=%B\n", x);
	printf("��������������˽Կ��������\n");
	element_mul_zn(Su, Du, x);
	element_printf("��ȫ˽Կ Su=%B\n", Su);
	printf("�������������ù�Կ��������\n");
	element_mul_zn(Xu, P, x);//Xu=xP
	element_mul_zn(Yu, Ppub, x);//Yu=xP
	printf("��ԿΪ��\n");
	element_printf("Xu=%B\n", Xu);
	element_printf("Yu=%B\n", Yu);
	printf("�������������������������ܽ׶Ρ�������������������\n");
	pairing_apply(T1, Xu, Ppub, pairing);//T1=e(Xu,Ppub)
	pairing_apply(T2, Yu, P, pairing);//T2=e(Yu,P)
									  //�жϹ�Կ�Ƿ���ȷ
	if (!element_cmp(T1, T2)) {
		element_random(r);
		element_mul_zn(V, P, r);//V=rPע��˳��
		pairing_apply(T1, Yu, Qu, pairing);//T1=e(Yu,Qu)
		element_pow_zn(T1, T1, r);//T1^r
		element_printf("V=%B\n", V);
		element_printf("e(Yu,Qu)^r=%B\n", T1);
	}
	else
	{
		printf("���� ��Կ����ȷ\n");
		exit(1);
	}
	printf("�������������������������ܽ׶Ρ�������������������\n");
	pairing_apply(T2, V, Su, pairing);
	element_printf("e(V,Su)=%B\n", T2);
	byte = element_length_in_bytes(V);
	printf("�����ܹ��ֽڳ���Ϊ%d\n", byte + 128);
	return 0;
}