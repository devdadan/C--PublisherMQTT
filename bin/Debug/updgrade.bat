cd /d "D:\VB\REG2Publisher\bin\Debug"
                                        taskkill /f /im reg2*
                                        del /f /q Reg2Publink.exe
                                        del /f /q Reg2Publink.pdb
                                        wget -P "D:\VB\REG2Publisher\bin\Debug" --user=Backoff --password=123456 ftp://192.168.190.100:21/Prog/REG2PubLink.zip -N
                                        unzip -o REG2PubLink.zip
                                        Reg2PubLink.exe
                                        exit