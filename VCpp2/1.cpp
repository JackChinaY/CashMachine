#include "stdafx.h"
#include <iostream>
using namespace std;

int main()
{
	char*a[] = { "work","at","alibaba" };
	char**pa = a;
	pa++;
	printf("%s", *pa);
	/*int x, y;
	while (true)
	{
		cout << "����������������(�ո�ָ�)��";
		cin >> x >> y;
		if (x <= 0 || y <= 0)
		{
			cout << endl << "�����д����������롣" << endl;
		}
		else
		{
			break;
		}
	}
	cout << x << "+" << y << "=" << x + y << endl;
	cin.get();
	system("pause");*/
	return 0;
}