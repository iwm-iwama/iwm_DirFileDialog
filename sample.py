#!python3
#coding:utf-8

import subprocess

CMD = ".\\iwm_DirFileDialog.exe -t=m"

# [重要]
subprocess.Popen("chcp 65001 >NUL", shell=True)

def SubDialog():
	while(True):
		child = subprocess.Popen(
			CMD,
			shell=True,
			stdout=subprocess.PIPE,
			stderr=subprocess.PIPE
		)
		stdout, stderr = child.communicate()
		if(len(stdout) == 0):
			break
		# [重要]
		print(stdout.decode("cp65001"), end="")

SubDialog()
