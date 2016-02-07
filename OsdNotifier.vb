Imports Microsoft.VisualBasic.Parallel

''' <summary>
''' 编程的API接口
''' </summary>
Public Class OsdNotifier : Implements System.IDisposable

    ''' <summary>
    ''' The message data will displayed on the osd bubble.
    ''' </summary>
    <Serializable> Public Class Message

        ''' <summary>
        ''' The title of the message.
        ''' </summary>
        ''' <returns></returns>
        <Xml.Serialization.XmlAttribute> Public Property Title As String
        <Xml.Serialization.XmlElement> Public Property Message As String
        ''' <summary>
        ''' The icon image file path.
        ''' </summary>
        ''' <returns></returns>
        <Xml.Serialization.XmlElement> Public Property IconURL As String
        ''' <summary>
        ''' The format of the bubble sound is limited on *.wav
        ''' </summary>
        ''' <returns></returns>
        <Xml.Serialization.XmlElement> Public Property SoundURL As String
        ''' <summary>
        ''' This handle indicated that when user click on the bubble what action will be run?
        ''' </summary>
        ''' <returns></returns>
        <Xml.Serialization.XmlIgnore> Public Property CallbackHandle As Action
        ''' <summary>
        ''' The displaying behavior of the osd bubble on the screen.
        ''' </summary>
        ''' <returns></returns>
        <Xml.Serialization.XmlElement> Public Property BubbleBehavior As BubbleBehaviorTypes

        Public ReadOnly Property Icon As Image
            Get
                If String.IsNullOrEmpty(_IconURL) OrElse Not FileIO.FileSystem.FileExists(_IconURL) Then
                    Return My.Resources.UBUNTU
                Else
                    Try
                        Return LoadImage(_IconURL)
                    Catch ex As Exception
                        Call Console.WriteLine(ex.ToString)
                        Call Debug.WriteLine(ex.ToString)
                        Call Trace.WriteLine(ex.ToString)
                        Return My.Resources.UBUNTU
                    End Try
                End If
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return Message
        End Function

        Public Function Copy(OverridesHandle As Action) As Message
            Return New Message With {
                .CallbackHandle = OverridesHandle,
                .Message = Message,
                .IconURL = IconURL,
                .Title = Title,
                .BubbleBehavior = BubbleBehavior,
                .SoundURL = SoundURL
            }
        End Function
    End Class

    Public Enum BubbleBehaviorTypes
        ''' <summary>
        ''' The message bubble will be closed automatically after a time period.
        ''' </summary>
        AutoClose
        ''' <summary>
        ''' The message bubble will not be closed until user click on it.
        ''' </summary>
        FreezUntileClick
        ''' <summary>
        ''' The message bubble will running as a process bar indicator, the bubble will automatically closed when the process value is 100.
        ''' </summary>
        ProcessIndicator
        ''' <summary>
        ''' When the user scrolling his mouse wheel on the bubble, then the action will be callback to adjust the value.
        ''' </summary>
        ValueAdjustments
    End Enum

    Dim _messageQueueList As Queue(Of OsdNotifier.Message) = New Queue(Of OsdNotifier.Message)

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

Public Class ProcessBarBubble

    Public Property PercentageValue As Integer
        Get
            Return Me._ProcBubble.Value
        End Get
        Set(value As Integer)
            Me._ProcBubble.Value = value
        End Set
    End Property

    Public ReadOnly Property ProcessingBar As ProcessingBar
        Get
            Return Me._ProcBubble.ProcessingBar
        End Get
    End Property

    Public Sub Cancel()
        Call Me._ProcBubble.Cancel()
    End Sub

    Dim _InvokeThread As Threading.Thread

    Public Sub ShowMessage(Percentage As Integer, MSG As OsdNotifier.Message)
        Me._ProcBubble.Value = Percentage
        Me._ProcBubble.Message = MSG
    End Sub

    Public Sub ShowMessage(Percentage As Integer, MSG As String)
        Me._ProcBubble.Value = Percentage
        Me._ProcBubble.Message = New OsdNotifier.Message With {.Message = MSG, .Title = Me._ProcBubble.Message.Title,
            .BubbleBehavior = Me._ProcBubble.Message.BubbleBehavior,
            .CallbackHandle = Me._ProcBubble.Message.CallbackHandle,
            .IconURL = Me._ProcBubble.Message.IconURL,
            .SoundURL = Me._ProcBubble.Message.SoundURL}
    End Sub

    Public Sub Show()
        If _InvokeThread Is Nothing Then
            Me._InvokeThread = New Threading.Thread(AddressOf Me._ProcBubble.ShowDialog)
            Call Me._InvokeThread.Start()
        End If
    End Sub

    Dim _ProcBubble As FormOsdProcessIndicator

    Sub New(MSG As OsdNotifier.Message, ScreenOffset As Point)
        Me._ProcBubble = New FormOsdProcessIndicator()
        Me._ProcBubble.ScreenOffSet = ScreenOffset
        Me._ProcBubble.Message = MSG

    End Sub

End Class

Public Class ValueAdjustments

    Public Property PercentageValue As Integer
        Get
            Return Me._AdjustmentBar.Value
        End Get
        Set(value As Integer)
            Me._AdjustmentBar.Value = value
        End Set
    End Property

    Dim _AdjustmentBar As FormOsdValueAdjustments

    Dim _InvokeThread As Threading.Thread

    Public Sub Show()
        If _InvokeThread Is Nothing Then
            Me._InvokeThread = New Threading.Thread(AddressOf Me._AdjustmentBar.ShowDialog)
            Call Me._InvokeThread.Start()
        End If
    End Sub

    Public ReadOnly Property ProcessingBar As ProcessingBar
        Get
            Return Me._AdjustmentBar.ProcessingBar
        End Get
    End Property

#Region "Constructors"

    Sub New(MSG As OsdNotifier.Message, Up As ValueAdjustmentInvoke, Down As ValueAdjustmentInvoke, ValueChanged As ValueAdjustmentInvoke)
        Me._AdjustmentBar = New FormOsdValueAdjustments(Up, Down, ValueChanged)
        Me._AdjustmentBar.Message = MSG
    End Sub

    Sub New(MSG As OsdNotifier.Message, Up As ValueAdjustmentInvoke, Down As ValueAdjustmentInvoke)
        Me._AdjustmentBar = New FormOsdValueAdjustments(Up, Down)
        Me._AdjustmentBar.Message = MSG
    End Sub

    Sub New(MSG As OsdNotifier.Message, ValueChanged As ValueAdjustmentInvoke)
        Me._AdjustmentBar = New FormOsdValueAdjustments(ValueChanged)
        Me._AdjustmentBar.Message = MSG
    End Sub
#End Region
End Class