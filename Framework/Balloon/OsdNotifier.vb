Imports Microsoft.VisualBasic.Parallel

''' <summary>
''' 编程的API接口
''' </summary>
Public Class OsdNotifier : Implements System.IDisposable

    Dim _messageQueueList As Queue(Of Message) = New Queue(Of Message)

    Sub New()
        Call Microsoft.VisualBasic.Parallel.Run(AddressOf __sendMessageThread)
    End Sub

    Private Sub __sendMessageThread()
        Do While Not Me.disposedValue
            Call Threading.Thread.Sleep(10)

            If _messageQueueList.IsNullOrEmpty Then
                Continue Do
            End If

            Dim Message = _messageQueueList.Dequeue
            Dim Notifier As New FormOsdNotify(Me) With
                {
                    .Message = Message,
                    .ActionCallback = Message.CallbackHandle
            }
            If Message.SoundURL.FileExists Then
                Call New Threading.Thread(Sub() Call WinMM.PlaySound(Message.SoundURL)).Start()
            End If

            Call Notifier.ShowDialog()
        Loop
    End Sub

    ''' <summary>
    ''' 这个函数只会将消息插入到队列之中
    ''' </summary>
    ''' <param name="Msg"></param>
    ''' <param name="CallbackHandle">假若用户点击了气泡并且合格参数不为空值的话，则会发生一个回调事件</param>
    Public Sub SendMessage(Msg As Message, Optional CallbackHandle As Action = Nothing)
        Call _messageQueueList.Enqueue(Msg.Copy(OverridesHandle:=CallbackHandle))
    End Sub

    Public Sub SendMessage(Title As String, Message As String, Icon As String, CallbackHandle As Action,
                           Optional Sound As String = "",
                           Optional Behavior As BubbleBehaviorTypes = BubbleBehaviorTypes.AutoClose)

        Dim Msg As Message = New Message With {
            .SoundURL = Sound,
            .Title = Title,
            .Message = Message,
            .IconURL = Icon,
            .CallbackHandle = CallbackHandle,
            .BubbleBehavior = Behavior
        }
        Call _messageQueueList.Enqueue(Msg)
    End Sub

    Public Sub SendMessage(Title As String, Message As String, Icon As Image, CallbackHandle As Action,
                           Optional Sound As String = "",
                           Optional Behavior As BubbleBehaviorTypes = BubbleBehaviorTypes.AutoClose)
        Dim iconUrl As String = FileIO.FileSystem.GetTempFileName
        Call Icon.Save(iconUrl)
        Dim Msg As Message = New Message With {
            .SoundURL = Sound,
            .Title = Title,
            .Message = Message,
            .IconURL = iconUrl,
            .CallbackHandle = CallbackHandle,
            .BubbleBehavior = Behavior
        }
        Call _messageQueueList.Enqueue(Msg)
    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' TODO: set large fields to null.
        End If
        disposedValue = True
    End Sub

    ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        ' TODO: uncomment the following line if Finalize() is overridden above.
        ' GC.SuppressFinalize(Me)
    End Sub
#End Region
End Class
