# -*- coding: utf-8 -*-
# Generated by the protocol buffer compiler.  DO NOT EDIT!
# source: DanmuGameProtocol.proto
"""Generated protocol buffer code."""
from google.protobuf.internal import enum_type_wrapper
from google.protobuf import descriptor as _descriptor
from google.protobuf import descriptor_pool as _descriptor_pool
from google.protobuf import message as _message
from google.protobuf import reflection as _reflection
from google.protobuf import symbol_database as _symbol_database
# @@protoc_insertion_point(imports)

_sym_db = _symbol_database.Default()




DESCRIPTOR = _descriptor_pool.Default().AddSerializedFile(b'\n\x17\x44\x61nmuGameProtocol.proto\x12\x11\x44\x61nmuGameProtocol\"\xb4\x01\n\x08MainPack\x12\x33\n\x0brequestCode\x18\x01 \x01(\x0e\x32\x1e.DanmuGameProtocol.RequestCode\x12\x31\n\nactionCode\x18\x02 \x01(\x0e\x32\x1d.DanmuGameProtocol.ActionCode\x12\x31\n\nreturnCode\x18\x03 \x01(\x0e\x32\x1d.DanmuGameProtocol.ReturnCode\x12\r\n\x05value\x18\x04 \x01(\x05*\x1e\n\x0bRequestCode\x12\x0f\n\x0bRequestNone\x10\x00*\x1c\n\nActionCode\x12\x0e\n\nActionNone\x10\x00*\x1c\n\nReturnCode\x12\x0e\n\nReturnNone\x10\x00\x62\x06proto3')

_REQUESTCODE = DESCRIPTOR.enum_types_by_name['RequestCode']
RequestCode = enum_type_wrapper.EnumTypeWrapper(_REQUESTCODE)
_ACTIONCODE = DESCRIPTOR.enum_types_by_name['ActionCode']
ActionCode = enum_type_wrapper.EnumTypeWrapper(_ACTIONCODE)
_RETURNCODE = DESCRIPTOR.enum_types_by_name['ReturnCode']
ReturnCode = enum_type_wrapper.EnumTypeWrapper(_RETURNCODE)
RequestNone = 0
ActionNone = 0
ReturnNone = 0


_MAINPACK = DESCRIPTOR.message_types_by_name['MainPack']
MainPack = _reflection.GeneratedProtocolMessageType('MainPack', (_message.Message,), {
  'DESCRIPTOR' : _MAINPACK,
  '__module__' : 'DanmuGameProtocol_pb2'
  # @@protoc_insertion_point(class_scope:DanmuGameProtocol.MainPack)
  })
_sym_db.RegisterMessage(MainPack)

if _descriptor._USE_C_DESCRIPTORS == False:

  DESCRIPTOR._options = None
  _REQUESTCODE._serialized_start=229
  _REQUESTCODE._serialized_end=259
  _ACTIONCODE._serialized_start=261
  _ACTIONCODE._serialized_end=289
  _RETURNCODE._serialized_start=291
  _RETURNCODE._serialized_end=319
  _MAINPACK._serialized_start=47
  _MAINPACK._serialized_end=227
# @@protoc_insertion_point(module_scope)