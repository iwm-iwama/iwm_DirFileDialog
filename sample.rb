#!ruby
#coding:utf-8

CMD = ".\\iwm_DirFileDialog.exe -t=m"

# [重要]
system "chcp 65001 >NUL"

def SubDialog()
	while true
		s1 = %x(#{CMD})
		break if s1.length == 0
		print s1
	end
end

SubDialog()
