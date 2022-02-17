import test_pb2
  
testinfo = test_pb2.testinfo()  
testinfo.devtype = 100  
testinfo.devid = 2  
testinfo.unitid = 3  
testinfo.chlid = 4  
testinfo.testid = 250
testinfo.stepdata = b'abd'

print(testinfo)  # 打印 protobuf 结构的内容
print(testinfo.devtype)

# out = testinfo.SerializeToString()  
# print(out)  # 打印 Protobuf 序列字符串

# decode = test_pb2.testinfo()  
# decode.ParseFromString(out)  
  
# print(decode) # 打印 解析Protobuf后的内容