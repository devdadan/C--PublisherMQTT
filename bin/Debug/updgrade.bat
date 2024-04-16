cd /d "D:\PROGRAM VB\DS-Projek\REG2Publisher\bin\Debug"
                                        taskkill /f /im reg2*
                                        del /f /q Reg2Publink.exe
                                        del /f /q Reg2Publink.pdb
                                        wget -P "D:\PROGRAM VB\DS-Projek\REG2Publisher\bin\Debug" --user=Backoff --password=123456 ftp://192.168.190.37:21/Reg2PubLink.zip -N
                                        unzip -o Reg2PubLink.zip
                                        Reg2PubLink.exe
                                        exit