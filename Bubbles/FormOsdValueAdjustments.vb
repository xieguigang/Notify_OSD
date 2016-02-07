Friend Class FormOsdValueAdjustments

    Dim Up As ValueAdjustmentInvoke, Down As ValueAdjustmentInvoke, ValueChanged As ValueAdjustmentInvoke

#Region "Constructors"

    Sub New(Up As ValueAdjustmentInvoke, Down As ValueAdjustmentInvoke, ValueChanged As ValueAdjustmentInvoke)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        Me.ValueChanged = ValueChanged
        Me.Up = Up
        Me.Down = Down

    End Sub

    Sub New(Up As ValueAdjustmentInvoke, Down As ValueAdjustmentInvoke)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        Me.Up = Up
        Me.Down = Down
    End Sub

    Sub New(ValueChanged As ValueAdjustmentInvoke)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        Me.ValueChanged = ValueChanged
    End Sub
#End Region

    Protected Overrides Sub AfterMessageDrawing()
        Me._resWidth = Me._resNormal.Width
        Me._OSD_MSG.BubbleBehavior = OsdNotifier.BubbleBehaviorTypes.ValueAdjustments
        Me.ProcessingBar.StopRollAnimation()
        Me.ProcessingBar.InvokeAnimation()
    End Sub

    Protected Overrides Sub BubbleMouseWheel(sender As Object, e As MouseEventArgs)

        Me._UnloadCount = 0

        Call Me.Increase(e.Delta / 100)

        If e.Delta > 0 Then
            If Not Me.Up Is Nothing Then
                Call Me.Up(Value)
            End If
        Else
            If Not Me.Down Is Nothing Then
                Call Me.Down(Value)
            End If
        End If

        If Not Me.ValueChanged Is Nothing Then
            Call Me.ValueChanged(Me.Value)
        End If
    End Sub

    Protected Overrides Function InternalBlur(res As Image) As Image
        Return GDIGaussBlur.GaussBlur(res)
    End Function

    Protected Overrides Sub MouseMoveEnterBlur(sender As Object, e As EventArgs)
        _Blur = True
        If _StartFadeOut Then Return
        PictureBox1.BackgroundImage = InvokeSetBlur()
        _BubbleFadeAnimationInvoker.Interval = 5
        _BubbleFadeAnimationInvoker.Enabled = True
        _TimerUnloadCount.Enabled = False
    End Sub

End Class

''' <summary>
''' 
''' </summary>
''' <param name="value">1 - 100</param>
Public Delegate Sub ValueAdjustmentInvoke(value As Integer)
