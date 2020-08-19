Imports System.Drawing
Imports System.Threading
Imports Microsoft.VisualBasic.Parallel

Namespace Framework.Balloon

    ''' <summary>
    ''' 编程的API接口
    ''' </summary>
    Public Class OsdNotifier : Implements IDisposable

        Dim messageQueue As New Queue(Of (message As Message, callback As Action))

        Sub New()
            Call RunTask(AddressOf showMessageLoop)
        End Sub

        Private Sub showMessageLoop()
            Do While Not Me.disposedValue
                Call Thread.Sleep(10)

                If Not messageQueue.IsNullOrEmpty Then
                    Call doShowMessage()
                End If
            Loop
        End Sub

        Private Sub doShowMessage()
            Dim msg = messageQueue.Dequeue
            Dim notifier As New FormOsdNotify(Me) With {
                .Message = msg.message,
                .ActionCallback = msg.callback
            }
            If msg.message.sound.FileExists Then
                Call RunTask(Sub() Call WinMM.PlaySound(msg.message.sound))
            End If

            Call notifier.ShowDialog()
        End Sub

        ''' <summary>
        ''' 这个函数只会将消息插入到队列之中
        ''' </summary>
        ''' <param name="Msg"></param>
        ''' <param name="callbacks">假若用户点击了气泡并且合格参数不为空值的话，则会发生一个回调事件</param>
        Public Sub SendMessage(Msg As Message, Optional callbacks As Action = Nothing)
            Call messageQueue.Enqueue((Msg, callbacks))
        End Sub

        Public Sub SendMessage(title As String, message As String, icon As String,
                               Optional callbacks As Action = Nothing,
                               Optional Sound As String = "",
                               Optional Behavior As BubbleBehaviors = BubbleBehaviors.AutoClose)

            Dim Msg As New Message With {
                .sound = Sound,
                .title = title,
                .message = message,
                .icon = icon,
                .behaviors = Behavior
            }
            Call messageQueue.Enqueue((Msg, callbacks))
        End Sub

        Public Sub SendMessage(Title As String, Message As String, Icon As Image, CallbackHandle As Action,
                               Optional Sound As String = "",
                               Optional Behavior As BubbleBehaviors = BubbleBehaviors.AutoClose)
            Dim iconUrl As String = FileIO.FileSystem.GetTempFileName
            Call Icon.Save(iconUrl)
            Dim Msg As New Message With {
                .sound = Sound,
                .title = Title,
                .message = Message,
                .icon = iconUrl,
                .behaviors = Behavior
            }
            Call messageQueue.Enqueue((Msg, CallbackHandle))
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
End Namespace