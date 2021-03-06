// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: DanmuGameProtocol.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace DanmuGameProtocol {

  /// <summary>Holder for reflection information generated from DanmuGameProtocol.proto</summary>
  public static partial class DanmuGameProtocolReflection {

    #region Descriptor
    /// <summary>File descriptor for DanmuGameProtocol.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static DanmuGameProtocolReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChdEYW5tdUdhbWVQcm90b2NvbC5wcm90bxIRRGFubXVHYW1lUHJvdG9jb2wi",
            "UwoITWFpblBhY2sSEwoLTWVzc2FnZVR5cGUYASABKA0SDgoGVXNlcklEGAIg",
            "ASgFEhAKCFVzZXJOYW1lGAMgASgJEhAKCFVzZXJUZXh0GAQgASgJKj4KC01l",
            "c3NhZ2VUeXBlEgsKB2Rhbm1ha3UQABIICgRnaWZ0EAESCQoFZ3VhcmQQAhIN",
            "CglzdXBlcmNoYXQQA2IGcHJvdG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(new[] {typeof(global::DanmuGameProtocol.MessageType), }, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::DanmuGameProtocol.MainPack), global::DanmuGameProtocol.MainPack.Parser, new[]{ "MessageType", "UserID", "UserName", "UserText" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Enums
  public enum MessageType {
    [pbr::OriginalName("danmaku")] Danmaku = 0,
    [pbr::OriginalName("gift")] Gift = 1,
    [pbr::OriginalName("guard")] Guard = 2,
    [pbr::OriginalName("superchat")] Superchat = 3,
  }

  #endregion

  #region Messages
  public sealed partial class MainPack : pb::IMessage<MainPack>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<MainPack> _parser = new pb::MessageParser<MainPack>(() => new MainPack());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<MainPack> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::DanmuGameProtocol.DanmuGameProtocolReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public MainPack() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public MainPack(MainPack other) : this() {
      messageType_ = other.messageType_;
      userID_ = other.userID_;
      userName_ = other.userName_;
      userText_ = other.userText_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public MainPack Clone() {
      return new MainPack(this);
    }

    /// <summary>Field number for the "MessageType" field.</summary>
    public const int MessageTypeFieldNumber = 1;
    private uint messageType_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public uint MessageType {
      get { return messageType_; }
      set {
        messageType_ = value;
      }
    }

    /// <summary>Field number for the "UserID" field.</summary>
    public const int UserIDFieldNumber = 2;
    private int userID_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int UserID {
      get { return userID_; }
      set {
        userID_ = value;
      }
    }

    /// <summary>Field number for the "UserName" field.</summary>
    public const int UserNameFieldNumber = 3;
    private string userName_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string UserName {
      get { return userName_; }
      set {
        userName_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "UserText" field.</summary>
    public const int UserTextFieldNumber = 4;
    private string userText_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string UserText {
      get { return userText_; }
      set {
        userText_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as MainPack);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(MainPack other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (MessageType != other.MessageType) return false;
      if (UserID != other.UserID) return false;
      if (UserName != other.UserName) return false;
      if (UserText != other.UserText) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (MessageType != 0) hash ^= MessageType.GetHashCode();
      if (UserID != 0) hash ^= UserID.GetHashCode();
      if (UserName.Length != 0) hash ^= UserName.GetHashCode();
      if (UserText.Length != 0) hash ^= UserText.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (MessageType != 0) {
        output.WriteRawTag(8);
        output.WriteUInt32(MessageType);
      }
      if (UserID != 0) {
        output.WriteRawTag(16);
        output.WriteInt32(UserID);
      }
      if (UserName.Length != 0) {
        output.WriteRawTag(26);
        output.WriteString(UserName);
      }
      if (UserText.Length != 0) {
        output.WriteRawTag(34);
        output.WriteString(UserText);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (MessageType != 0) {
        output.WriteRawTag(8);
        output.WriteUInt32(MessageType);
      }
      if (UserID != 0) {
        output.WriteRawTag(16);
        output.WriteInt32(UserID);
      }
      if (UserName.Length != 0) {
        output.WriteRawTag(26);
        output.WriteString(UserName);
      }
      if (UserText.Length != 0) {
        output.WriteRawTag(34);
        output.WriteString(UserText);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (MessageType != 0) {
        size += 1 + pb::CodedOutputStream.ComputeUInt32Size(MessageType);
      }
      if (UserID != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(UserID);
      }
      if (UserName.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(UserName);
      }
      if (UserText.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(UserText);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(MainPack other) {
      if (other == null) {
        return;
      }
      if (other.MessageType != 0) {
        MessageType = other.MessageType;
      }
      if (other.UserID != 0) {
        UserID = other.UserID;
      }
      if (other.UserName.Length != 0) {
        UserName = other.UserName;
      }
      if (other.UserText.Length != 0) {
        UserText = other.UserText;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 8: {
            MessageType = input.ReadUInt32();
            break;
          }
          case 16: {
            UserID = input.ReadInt32();
            break;
          }
          case 26: {
            UserName = input.ReadString();
            break;
          }
          case 34: {
            UserText = input.ReadString();
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 8: {
            MessageType = input.ReadUInt32();
            break;
          }
          case 16: {
            UserID = input.ReadInt32();
            break;
          }
          case 26: {
            UserName = input.ReadString();
            break;
          }
          case 34: {
            UserText = input.ReadString();
            break;
          }
        }
      }
    }
    #endif

  }

  #endregion

}

#endregion Designer generated code
