﻿using System;
using System.Collections.Generic;
using HomegearLib.RPC;

namespace HomegearLib
{
    public enum VariableType
    {
        tBoolean,
        tAction,
        tInteger,
        tDouble,
        tString,
        tEnum
    }

    public enum VariableUIFlags
    {
        fNone = 0,
        fVisible = 1,
        fInternal = 2,
        fTransform = 4,
        fService = 8,
        fSticky = 16
    }

    public class Variable : IDisposable
    {
        protected RPCController _rpc = null;

        protected VariableType _type = VariableType.tInteger;
        public VariableType Type { get { return _type; } internal set { _type = value; } }

        protected long _peerId = 0;
        public long PeerID { get { return _peerId; } internal set { _peerId = value; } }

        protected long _channel = -1;
        public long Channel { get { return _channel; } internal set { _channel = value; } }

        protected string _name = "";
        public string Name { get { return _name; } internal set { _name = value; } }

        protected string _unit = "";
        public string Unit { get { return _unit; } internal set { _unit = value; } }

        protected bool _defaultBoolean = false;
        public bool DefaultBoolean { get { return _defaultBoolean; } internal set { _defaultBoolean = value; } }

        protected long _defaultInteger = 0;
        public long DefaultInteger { get { return _defaultInteger; } internal set { _defaultInteger = value; } }

        protected double _defaultDouble = 0;
        public double DefaultDouble { get { return _defaultDouble; } internal set { _defaultDouble = value; } }

        protected string _defaultString = "";
        public string DefaultString { get { return _defaultString; } internal set { _defaultString = value; } }

        protected long _minInteger = 0;
        public long MinInteger { get { return _minInteger; } internal set { _minInteger = value; } }

        protected long _maxInteger = 0;
        public long MaxInteger { get { return _maxInteger; } internal set { _maxInteger = value; } }

        protected double _minDouble = 0;
        public double MinDouble { get { return _minDouble; } internal set { _minDouble = value; } }

        protected double _maxDouble = 0;
        public double MaxDouble { get { return _maxDouble; } internal set { _maxDouble = value; } }

        protected VariableUIFlags _uiFlags = VariableUIFlags.fNone;
        public VariableUIFlags UIFlags { get { return _uiFlags; } internal set { _uiFlags = value; } }

        protected bool _readable = true;
        public bool Readable { get { return _readable; } internal set { _readable = value; } }

        protected bool _writeable = true;
        public bool Writeable { get { return _writeable; } internal set { _writeable = value; } }

        protected ReadOnlyDictionary<long, string> _specialIntegerValues = new ReadOnlyDictionary<long, string>();
        public ReadOnlyDictionary<long, string> SpecialIntegerValues { get { return _specialIntegerValues; } }

        protected ReadOnlyDictionary<double, string> _specialDoubleValues = new ReadOnlyDictionary<double, string>();
        public ReadOnlyDictionary<double, string> SpecialDoubleValues { get { return _specialDoubleValues; } }

        protected bool _setValueWait = false;
        public bool SetValueWait { get { return _setValueWait; } set { _setValueWait = value; } }

        protected bool _booleanValue = false;
        public virtual bool BooleanValue
        {
            get
            {
                return _booleanValue;
            }
            set
            {
                if (_rpc == null)
                {
                    throw new HomegearVariableException("No RPC controller specified.");
                }

                if (!_writeable)
                {
                    throw new HomegearVariableReadOnlyException("Variable " + _name + " is readonly");
                }

                if (_type != VariableType.tBoolean && _type != VariableType.tAction)
                {
                    throw new HomegearVariableTypeException("Variable " + _name + " is not of type boolean or action.");
                }

                _booleanValue = value;
                _rpc.SetValue(this);
            }
        }

        protected long _integerValue = 0;
        public virtual long IntegerValue
        {
            get
            {
                return _integerValue;
            }
            set
            {
                if (_rpc == null)
                {
                    throw new HomegearVariableException("No RPC controller specified.");
                }

                if (!_writeable)
                {
                    throw new HomegearVariableReadOnlyException("Variable " + _name + " is readonly");
                }

                if (_type != VariableType.tInteger && _type != VariableType.tEnum)
                {
                    throw new HomegearVariableTypeException("Variable " + _name + " is not of type integer or enum.");
                }

                if ((value > _maxInteger || value < _minInteger) && !_specialIntegerValues.ContainsKey(value))
                {
                    throw new HomegearVariableValueOutOfBoundsException("Value of variable " + _name + " is out of bounds.");
                }

                _integerValue = value;
                _rpc.SetValue(this);
            }
        }

        protected double _doubleValue = 0;
        public virtual double DoubleValue
        {
            get
            {
                return _doubleValue;
            }
            set
            {
                if (_rpc == null)
                {
                    throw new HomegearVariableException("No RPC controller specified.");
                }

                if (!_writeable)
                {
                    throw new HomegearVariableReadOnlyException("Variable " + _name + " is readonly");
                }

                if (_type != VariableType.tDouble)
                {
                    throw new HomegearVariableTypeException("Variable " + _name + " is not of type double.");
                }

                if ((value > _maxDouble || value < _minDouble) && !_specialDoubleValues.ContainsKey(value))
                {
                    throw new HomegearVariableValueOutOfBoundsException("Value of variable " + _name + " is out of bounds.");
                }

                _doubleValue = value;
                _rpc.SetValue(this);
            }
        }

        protected string _stringValue = "";
        public virtual string StringValue
        {
            get
            {
                return _stringValue;
            }
            set
            {
                if (_rpc == null)
                {
                    throw new HomegearVariableException("No RPC controller specified.");
                }

                if (!_writeable)
                {
                    throw new HomegearVariableReadOnlyException("Variable " + _name + " is readonly");
                }

                if (_type != VariableType.tString)
                {
                    throw new HomegearVariableTypeException("Variable " + _name + " is not of type string.");
                }

                if (_stringValue == null)
                {
                    _stringValue = "";
                }
                else
                {
                    _stringValue = value;
                }

                _rpc.SetValue(this);
            }
        }

        protected Dictionary<int, string> _valueList = new Dictionary<int, string>();
        public Dictionary<int, string> ValueList { get { return _valueList; } internal set { _valueList = value; } }

        public Variable(long peerId, long channel, string name) : this(null, peerId, channel, name)
        {
        }

        public Variable(RPCController rpc, long peerId, long channel, string name)
        {
            _rpc = rpc;
            _peerId = peerId;
            _channel = channel;
            _name = name;
        }

        internal Variable(long peerId, long channel, string name, RPCVariable rpcVariable) : this(null, peerId, channel, name, rpcVariable)
        {

        }

        internal Variable(long peerId, long channel, string name, string typeString, RPCVariable rpcVariable) : this(null, peerId, channel, name, typeString, rpcVariable)
        {

        }

        internal Variable(RPCController rpc, long peerId, long channel, string name, RPCVariable rpcVariable) : this(rpc, peerId, channel, name)
        {
            SetValue(rpcVariable);
        }

        internal Variable(RPCController rpc, long peerId, long channel, string name, string typeString, RPCVariable rpcVariable) : this(rpc, peerId, channel, name)
        {
            SetType(typeString);
            SetValue(rpcVariable);
        }

        public void Dispose()
        {
            _rpc = null;
        }

        internal void SetDefault(RPCVariable value)
        {
            if (value.Type == RPCVariableType.rpcBoolean)
            {
                _defaultBoolean = value.BooleanValue;
            }
            else if (value.Type == RPCVariableType.rpcInteger)
            {
                _defaultInteger = value.IntegerValue;
            }
            else if (value.Type == RPCVariableType.rpcFloat)
            {
                _defaultDouble = value.FloatValue;
            }
            else if (value.Type == RPCVariableType.rpcString)
            {
                if (value.StringValue != null)
                {
                    _defaultString = value.StringValue;
                }
            }
        }

        internal void SetMin(RPCVariable min)
        {
            if (min.Type == RPCVariableType.rpcInteger)
            {
                _minInteger = min.IntegerValue;
            }
            else if (min.Type == RPCVariableType.rpcFloat)
            {
                _minDouble = min.FloatValue;
            }
        }

        internal void SetMax(RPCVariable max)
        {
            if (max.Type == RPCVariableType.rpcInteger)
            {
                _maxInteger = max.IntegerValue;
            }
            else if (max.Type == RPCVariableType.rpcFloat)
            {
                _maxDouble = max.FloatValue;
            }
        }

        internal bool SetValue(RPCVariable rpcVariable)
        {
            bool changed = false;
            switch (rpcVariable.Type)
            {
                case RPCVariableType.rpcBoolean:
                    if (_booleanValue != rpcVariable.BooleanValue)
                    {
                        changed = true;
                    }

                    _booleanValue = rpcVariable.BooleanValue;
                    if (_type != VariableType.tAction)
                    {
                        _type = VariableType.tBoolean;
                    }

                    break;
                case RPCVariableType.rpcInteger:
                    if (_integerValue != rpcVariable.IntegerValue)
                    {
                        changed = true;
                    }

                    _integerValue = rpcVariable.IntegerValue;
                    if (_type != VariableType.tEnum && _type != VariableType.tInteger)
                    {
                        _type = VariableType.tInteger;
                    }

                    break;
                case RPCVariableType.rpcFloat:
                    if (_doubleValue != rpcVariable.FloatValue)
                    {
                        changed = true;
                    }

                    _doubleValue = rpcVariable.FloatValue;
                    _type = VariableType.tDouble;
                    break;
                case RPCVariableType.rpcString:
                    if (rpcVariable.StringValue == null)
                    {
                        rpcVariable.StringValue = "";
                    }

                    if (_stringValue != rpcVariable.StringValue)
                    {
                        changed = true;
                    }

                    _stringValue = rpcVariable.StringValue;
                    _type = VariableType.tString;
                    break;
            }
            return changed;
        }

        internal void SetType(string type)
        {
            switch (type)
            {
                case "ACTION":
                    _type = VariableType.tAction;
                    break;
                case "BOOL":
                    _type = VariableType.tBoolean;
                    break;
                case "INTEGER":
                    _type = VariableType.tInteger;
                    break;
                case "INTEGER64":
                    _type = VariableType.tInteger;
                    break;
                case "FLOAT":
                    _type = VariableType.tDouble;
                    break;
                case "STRING":
                    _type = VariableType.tString;
                    break;
                case "ENUM":
                    _type = VariableType.tEnum;
                    break;
            }
        }

        internal void SetValue(Variable variable)
        {
            switch (variable.Type)
            {
                case VariableType.tBoolean:
                    _booleanValue = variable.BooleanValue;
                    if (_type != VariableType.tAction)
                    {
                        _type = VariableType.tBoolean;
                    }

                    break;
                case VariableType.tAction:
                    _booleanValue = variable.BooleanValue;
                    _type = VariableType.tAction;
                    break;
                case VariableType.tInteger:
                    _integerValue = variable.IntegerValue;
                    if (_type != VariableType.tEnum)
                    {
                        _type = VariableType.tInteger;
                    }

                    break;
                case VariableType.tDouble:
                    _doubleValue = variable.DoubleValue;
                    _type = VariableType.tDouble;
                    break;
                case VariableType.tString:
                    if (variable.StringValue == null)
                    {
                        _stringValue = "";
                    }
                    else
                    {
                        _stringValue = variable.StringValue;
                    }

                    _type = VariableType.tString;
                    break;
                case VariableType.tEnum:
                    _integerValue = variable.IntegerValue;
                    break;
            }
        }

        internal void SetValueList(RPCVariable valueList)
        {
            if (_type == VariableType.tInteger)
            {
                _type = VariableType.tEnum;
            }

            _valueList = new Dictionary<int, string>();
            int offset = (int)((_minInteger < 0) ? _minInteger : 0);
            int x = 0;
            for (int i = (int)_minInteger; i < valueList.ArrayValue.Count + offset; i++, x++)
            {
                if (valueList.ArrayValue[x].Type != RPCVariableType.rpcString)
                {
                    continue;
                }

                _valueList[i] = valueList.ArrayValue[x].StringValue;
            }
        }

        internal void SetSpecialValues(RPCVariable specialValues)
        {
            Dictionary<long, string> specialIntegerValues = new Dictionary<long, string>();
            Dictionary<double, string> specialDoubleValues = new Dictionary<double, string>();
            foreach (RPCVariable specialValue in specialValues.ArrayValue)
            {
                if (!specialValue.StructValue.ContainsKey("ID") || !specialValue.StructValue.ContainsKey("VALUE"))
                {
                    continue;
                }

                RPCVariable value = specialValue.StructValue["VALUE"];
                if (value.Type == RPCVariableType.rpcInteger)
                {
                    specialIntegerValues.Add(value.IntegerValue, specialValue.StructValue["ID"].StringValue);
                }
                else if (value.Type == RPCVariableType.rpcInteger)
                {
                    specialIntegerValues.Add(value.IntegerValue, specialValue.StructValue["ID"].StringValue);
                }
                else if (value.Type == RPCVariableType.rpcFloat)
                {
                    specialDoubleValues.Add(value.FloatValue, specialValue.StructValue["ID"].StringValue);
                }
            }
            if (specialIntegerValues.Count > 0)
            {
                _specialIntegerValues = new ReadOnlyDictionary<long, string>(specialIntegerValues);
            }

            if (specialDoubleValues.Count > 0)
            {
                _specialDoubleValues = new ReadOnlyDictionary<double, string>(specialDoubleValues);
            }
        }

        public string DefaultToString()
        {
            switch (_type)
            {
                case VariableType.tBoolean:
                    return _defaultBoolean.ToString();
                case VariableType.tAction:
                    return _defaultBoolean.ToString();
                case VariableType.tInteger:
                    return _defaultInteger.ToString();
                case VariableType.tDouble:
                    return _defaultDouble.ToString();
                case VariableType.tString:
                    return _defaultString;
                case VariableType.tEnum:
                    return _defaultInteger.ToString();
            }
            return "";
        }

        public override string ToString()
        {
            switch (_type)
            {
                case VariableType.tBoolean:
                    return _booleanValue.ToString();
                case VariableType.tAction:
                    return _booleanValue.ToString();
                case VariableType.tInteger:
                    return _integerValue.ToString();
                case VariableType.tDouble:
                    return _doubleValue.ToString();
                case VariableType.tString:
                    return _stringValue;
                case VariableType.tEnum:
                    return _integerValue.ToString();
            }
            return "";
        }

        public bool Compare(Variable variable)
        {
            if (_type != variable.Type)
            {
                return false;
            }

            switch (_type)
            {
                case VariableType.tBoolean:
                    return _booleanValue == variable.BooleanValue;
                case VariableType.tAction:
                    return _booleanValue == variable.BooleanValue;
                case VariableType.tInteger:
                    return _integerValue == variable.IntegerValue;
                case VariableType.tDouble:
                    return _doubleValue == variable.DoubleValue;
                case VariableType.tString:
                    return _stringValue == variable.StringValue;
                case VariableType.tEnum:
                    return _integerValue == variable.IntegerValue;
            }
            return true;
        }
    }
}
