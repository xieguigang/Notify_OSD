Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Serialization.JSON

Module MessageRender

    ''' <summary>
    ''' Drawing the 8 parts of the image frame into one piece
    ''' </summary>
    ''' <param name="imageData"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function DrawFrame(imageData As Image) As Image
        Using g As Graphics2D = DirectCast(imageData.Clone, Image)
            Dim bottomHeight As Integer = My.Resources.NotificationMainPanel_bottom.Height,
                   topHeight As Integer = My.Resources.NotificationMainPanel_top.Height

            g.CompositingQuality = CompositingQuality.HighQuality

            Call g.DrawImage(My.Resources.NotificationMainPanel_top, 7, 0, imageData.Width - 14, topHeight)
            Call g.DrawImage(My.Resources.NotificationMainPanel_top_LEFT, 0, 0, 7, 7)
            Call g.DrawImage(My.Resources.NotificationMainPanel_top_RIGHT, imageData.Width - 7, 0, 7, 7)
            Call g.DrawImage(My.Resources.NotificationMainPanel_bottom, 7, imageData.Height - bottomHeight, imageData.Width - 14, bottomHeight)
            Call g.DrawImage(My.Resources.NotificationMainPanel_bottom_LEFT, 0, imageData.Height - 7, 7, 7)
            Call g.DrawImage(My.Resources.NotificationMainPanel_bottom_Right, imageData.Width - 7, imageData.Height - 7, 7, 7)

            Call g.DrawImage(My.Resources.PanelFrame, 0, topHeight, My.Resources.PanelFrame.Width, imageData.Height - topHeight - bottomHeight)
            Call g.DrawImage(My.Resources.PanelFrame, imageData.Width - My.Resources.PanelFrame.Width, topHeight, My.Resources.PanelFrame.Width, imageData.Height - topHeight - bottomHeight)

            Return g.ImageResource
        End Using
    End Function

    ''' <summary>
    ''' drawing the message title, message string and the message icon
    ''' </summary>
    ''' <param name="MSG"></param>
    ''' <returns></returns>
    ''' <remarks>函数依靠字符串中含有多少个回车符来判断绘制的位置</remarks>
    Public Function DrawMessage(MSG As Message, Renderer As RenderParameters) As Image
        Try
            Return drawMessageInternal(MSG, Renderer)
        Catch ex As Exception
            ex = New Exception("[Message] " & MSG.GetJson, ex)
            ex = New Exception("[Parameters] " & Renderer.GetJson, ex)

            Dim exMsg As String = VBDebugger.BugsFormatter(ex)
            Call FileIO.FileSystem.WriteAllText(App.HOME & "/notify-osd.log", exMsg & vbCrLf, append:=True)

            Return Nothing
        End Try
    End Function

    ReadOnly _textMeasures As Graphics = Graphics.FromImage(My.Resources.UBUNTU)
    ReadOnly _minSize As New Size(352, 73)

    Private Function drawMessageInternal(msg As Message, params As RenderParameters) As Image
        Dim title As String = If(String.IsNullOrEmpty(msg.title), Now.ToString, msg.title.Replace("\n", vbCrLf))
        Dim message As String = If(String.IsNullOrEmpty(msg.content), "", msg.content.Replace("\n", vbCrLf))
        Dim titleSize As SizeF = _textMeasures.MeasureString(title, params.TitleFont)
        Dim msgSize As SizeF = _textMeasures.MeasureString(message, params.MessageFont)
        Dim margins As New Size(15, 5)
        Dim grSize As New Point With {
            .X = Math.Max(titleSize.Width, msgSize.Width) + params.IconSize + margins.Width * 3,
            .Y = margins.Height * 2 + 5 + titleSize.Height + msgSize.Height + 25
        }

        If grSize.Y < 76 Then
            grSize = New Point(grSize.X, 76)
        End If
        If grSize.X < 352 Then
            grSize = New Point(352, grSize.Y)
        End If

        Using g As Graphics2D = New Size(grSize.X, grSize.Y + params.YDelta).CreateGDIDevice(Color.Transparent)
            g.Graphics.CompositingQuality = CompositingQuality.HighQuality

            '绘制表面纹理
            Call g.FillRegion(Brushes.Black, New Region(New Rectangle(New Point, grSize)))
            '绘制消息内容
            g.DrawImage(msg.GetIconImage, 15, 15, params.IconSize, params.IconSize)
            g.DrawString(title, params.TitleFont, Brushes.WhiteSmoke, New Point(78, 15))
            g.DrawString(message, params.MessageFont, Brushes.White, New Point(78, 5 + 15 + titleSize.Height))

            Return g.ImageResource
        End Using
    End Function
End Module
