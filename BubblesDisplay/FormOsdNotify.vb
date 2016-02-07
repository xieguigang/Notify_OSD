Imports System.Text

Friend Class FormOsdNotify

    ''' <summary>
    ''' Using for message dequeue
    ''' </summary>
    ''' <remarks></remarks>
    Public Event DisposeFinal()

    Protected DrawFadeAnimation As System.Action
    Protected _resNormal, _resHLBlur As Image
    Protected WithEvents _TimerUnloadCount As New Timers.Timer(800)
    Protected WithEvents _BubbleFadeAnimationInvoker As New Timers.Timer(1)
    Protected _OsdNotifier As OsdNotifier

    ''' <summary>
    ''' 根据传入的数据，格式化字符串之后生成底图
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable Property Message As Message
        Get
            Return Me._OSD_MSG
        End Get
        Set(value As Message)
            Me._OSD_MSG = value
            Me.Text = value.Title
            Call InvokeDrawing()
        End Set
    End Property

    Protected Sub InvokeDrawing()
        _resNormal = MessageRender.DrawMessage(Me._OSD_MSG, Me.GetRenderer)
        _resHLBlur = _resNormal
        _resHLBlur = GaussBlur.GaussBlur(GaussBlur.GaussBlur(GaussBlur.GaussBlur(_resHLBlur)))

        '绘制边框
        _resNormal = MessageRender.DrawFrame(_resNormal)
        _resHLBlur = MessageRender.DrawFrame(_resHLBlur)

        Me.PictureBox1.BackgroundImage = _resNormal
        Me.Size = _resNormal.Size

        Me.Location = New Point With {
            .X = Screen.PrimaryScreen.Bounds.Width - Me.Width - 10 + ScreenOffSet.X,
            .Y = 0.04 * Screen.PrimaryScreen.Bounds.Height + ScreenOffSet.Y
        }

        Call AfterMessageDrawing()
    End Sub

    Public Property ScreenOffSet As Point = New Point

    ''' <summary>
    ''' 是否允许鼠标的移动会阻止消息气泡的消失
    ''' </summary>
    ''' <returns></returns>
    Public Property EnableBubbleFadingMouseEvent As Boolean = True

    Protected Overridable Function GetRenderer() As MessageRender.MessageRender
        Return New MessageRender.MessageRender
    End Function

    Protected _OSD_MSG As Message

    Sub New(OsdNotifier As OsdNotifier)
        CheckForIllegalCrossThreadCalls = False
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me._OsdNotifier = OsdNotifier
    End Sub

    Protected Sub New()
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        CheckForIllegalCrossThreadCalls = False
        Me._OsdNotifier = New OsdNotifier
    End Sub

    Private Sub FormosdNotify_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Opacity = 0
        Me.TopMost = True
        _BubbleFadeAnimationInvoker.Interval = 1
        DrawFadeAnimation = AddressOf FadeIn
        _BubbleFadeAnimationInvoker.Enabled = True
        _TimerUnloadCount.Interval = 500
        _TimerUnloadCount.Enabled = True
    End Sub

    Protected Overridable Sub MouseMoveEnterBlur(sender As Object, e As EventArgs) Handles PictureBox1.MouseEnter
        If _StartFadeOut Then Return
        PictureBox1.BackgroundImage = InvokeSetBlur()
        _BubbleFadeAnimationInvoker.Interval = 5
        DrawFadeAnimation = AddressOf FadeBlur
        _BubbleFadeAnimationInvoker.Enabled = True
        _TimerUnloadCount.Enabled = False
    End Sub

    Private Sub FadeBlur()
        If Me.Opacity > 0.3 Then
            Me.Opacity -= 0.04
        Else
            _BubbleFadeAnimationInvoker.Enabled = False
        End If
    End Sub

    Protected Overridable Function InvokeSetBlur() As Image
        Return Me._resHLBlur
    End Function

    Protected Overridable Function InvokeSetNormal() As Image
        Return Me._resNormal
    End Function

    Protected Overridable Sub MouseLeaveBackNormal(sender As Object, e As EventArgs) Handles PictureBox1.MouseLeave
        If _StartFadeOut Then Return
        PictureBox1.BackgroundImage = InvokeSetNormal()
        _BubbleFadeAnimationInvoker.Interval = 1
        DrawFadeAnimation = AddressOf FadeIn
        _BubbleFadeAnimationInvoker.Enabled = True
        _TimerUnloadCount.Enabled = True
        _UnloadCount = 0
    End Sub

    Private Sub BubbleFadeAnimation(sender As Object, e As EventArgs) Handles _BubbleFadeAnimationInvoker.Elapsed
        Call DrawFadeAnimation()
    End Sub

    Protected _UnloadCount As Integer = 0, _StartFadeOut As Boolean = False

    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles _TimerUnloadCount.Elapsed
        Call Debug.WriteLine(Me.Message.BubbleBehavior.ToString)

        If Me.Message.BubbleBehavior = BubbleBehaviorTypes.FreezUntileClick OrElse
            Me._OSD_MSG.BubbleBehavior = BubbleBehaviorTypes.ProcessIndicator Then    '必须要走到100才可以，所以到100之后修改Message里面的这个属性为AutoClose即可
            _UnloadCount = 0
            Return
        End If

        If _UnloadCount < 8 Then
            _UnloadCount += 1
        Else
            Call InvokeStartFadeOut()
        End If
    End Sub

    ''' <summary>
    ''' 消息气泡从这里开始消失
    ''' </summary>
    Protected Overridable Sub InvokeStartFadeOut()
        _StartFadeOut = True
        _TimerUnloadCount.Enabled = False
        _BubbleFadeAnimationInvoker.Interval = 1
        DrawFadeAnimation = AddressOf FadeOut
        _BubbleFadeAnimationInvoker.Enabled = True
    End Sub

    ''' <summary>
    ''' Callback
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Protected Overridable Sub BubbleClick(sender As Object, e As EventArgs) Handles PictureBox1.Click
        Dim Handle = ActionCallback

        If Me._OSD_MSG.BubbleBehavior = BubbleBehaviorTypes.ProcessIndicator Then
            '必须要走到100才可以

        End If

        If Not Handle Is Nothing Then
            Call Handle()
            ActionCallback = Nothing
            Call InvokeStartFadeOut()
        End If
    End Sub

    Public Property ActionCallback As Action

    Private Sub FadeIn()
        If Me.Opacity < 0.9 Then
            Me.Opacity += 0.05
        Else
            _BubbleFadeAnimationInvoker.Enabled = False
        End If
    End Sub

    Private Sub FadeOut()
        If Me.Opacity > 0 Then
            Me.Opacity -= 0.06
        Else
            Call AfterCleanUp()
            Call Me.Close()
            _BubbleFadeAnimationInvoker.Enabled = False
        End If
    End Sub

#Region "Needs Overrides"

    Protected Overridable Sub AfterCleanUp()
        'Base class do nothing
    End Sub

    Protected Overridable Sub BubbleMouseWheel(sender As Object, e As MouseEventArgs) Handles PictureBox1.MouseWheel
        'Base class do nothing
    End Sub

    Protected Overridable Sub AfterMessageDrawing()
        'Base class do nothing
    End Sub
#End Region
End Class
