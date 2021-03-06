﻿using System;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
namespace LogTrace.Core
{
    /// <summary>
    /// 日志上下文
    /// </summary>
    public struct LoggerContext
    {
        /// <summary>
        /// 上下文字段
        /// </summary>
        private const string ContextField = nameof(LogTrace) + "."+ nameof(LoggerContext);

        /// <summary>
        /// 上下文中需要存储的值
        /// </summary>
        private object[] _values;

        private TraceEventType _minLevel;
        private Guid _contextId;
        private bool _isNew;
        private bool _isInitialized;
        /// <summary>
        /// 初始化
        /// </summary>
        private bool Initialize(bool create = true)
        {
            if (_isInitialized == false)
            {
                _values = (object[]) CallContext.LogicalGetData(ContextField);
                if (_values != null)
                {
                    _contextId = (Guid) _values[0];
                    _minLevel = (TraceEventType) _values[1];
                    _isNew = false;
                }
                else if (create)
                {
                    _contextId = Trace.CorrelationManager.ActivityId;
                    _minLevel = 0;
                    _isNew = true;
                    if (_contextId == Guid.Empty)
                    {
                        Trace.CorrelationManager.ActivityId = _contextId = Guid.NewGuid();
                    }
                    _values = new object[] { _contextId, _minLevel };
                    CallContext.LogicalSetData(ContextField, _values);
                }
                else
                {
                    return false;
                }
                _isInitialized = true;
            }
            return true;
        }

        /// <summary>
        /// 上下文中的日志最小等级
        /// </summary>
        public TraceEventType MinLevel
        {
            get
            {
                Initialize();
                return _minLevel;
            }
            set
            {
                Initialize();
                if ((_minLevel == 0) || (value < _minLevel))
                {
                    _values[1] = _minLevel = value;
                }
            }
        }

        /// <summary>
        /// 日志id
        /// </summary>
        public Guid ContextId
        {
            get
            {
                Initialize();
                return _contextId;
            }
            private set
            {
                Initialize();
                _contextId = value;
            }
        }

        /// <summary>
        /// 是否是一个新的上下文
        /// </summary>
        public bool IsNew
        {
            get
            {
                Initialize();
                return _isNew;
            }
            private set
            {
                Initialize();
                _isNew = value;
            }
        }

        /// <summary>
        /// 是否存在上下文
        /// </summary>
        public bool Exists => Initialize(false);

        /// <summary>
        /// 清除上下文
        /// </summary>
        public static void Clear() => CallContext.FreeNamedDataSlot(ContextField);
    }
}