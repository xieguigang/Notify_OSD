Imports System.Text

Module MessageRender

    ''' <summary>
    ''' Drawing the 8 parts of the image frame into one piece
    ''' </summary>
    ''' <param name="ImageData"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function DrawFrame(ImageData As Image) As Image
        Dim resIMG As Image = DirectCast(ImageData.Clone, Image)

        Using GraphicDevice As Graphics = Graphics.FromImage(resIMG)
            Dim BottomHeight As Integer = My.Resources.NotificationMainPanel_bottom.Height,
                   TopHeight As Integer = My.Resources.NotificationMainPanel_top.Height

            GraphicDevice.CompositingQuality = Drawing2D.CompositingQuality.HighQuality

            Call GraphicDevice.DrawImage(My.Resources.NotificationMainPanel_top, 7, 0, ImageData.Width - 14, TopHeight)
            Call GraphicDevice.DrawImage(My.Resources.NotificationMainPanel_top_LEFT, 0, 0, 7, 7)
            Call GraphicDevice.DrawImage(My.Resources.NotificationMainPanel_top_RIGHT, ImageData.Width - 7, 0, 7, 7)
            Call GraphicDevice.DrawImage(My.Resources.NotificationMainPanel_bottom, 7, ImageData.Height - BottomHeight, ImageData.Width - 14, BottomHeight)
            Call GraphicDevice.DrawImage(My.Resources.NotificationMainPanel_bottom_LEFT, 0, ImageData.Height - 7, 7, 7)
            Call GraphicDevice.DrawImage(My.Resources.NotificationMainPanel_bottom_Right, ImageData.Width - 7, ImageData.Height - 7, 7, 7)

            Call GraphicDevice.DrawImage(My.Resources.PanelFrame, 0, TopHeight, My.Resources.PanelFrame.Width, ImageData.Height - TopHeight - BottomHeight)
            Call GraphicDevice.DrawImage(My.Resources.PanelFrame, ImageData.Width - My.Resources.PanelFrame.Width, TopHeight, My.Resources.PanelFrame.Width, ImageData.Height - TopHeight - BottomHeight)

            Return resIMG
        End Using
    End Function

    ''' <summary>
    ''' drawing the message title, message string and the message icon
    ''' </summary>
    ''' <param name="MSG"></param>
    ''' <returns></returns>
    ''' <remarks>函数依靠字符串中含有多少个回车符来判断绘制的位置</remarks>
    Public Function DrawMessage(MSG As OsdNotifier.Message, Renderer As MessageRender) As Image
        Try
            Dim s_Title As String = If(String.IsNullOrEmpty(MSG.Title), Now.ToString, MSG.Title.Replace("\n", vbCrLf)),
                strMessage As String = If(String.IsNullOrEmpty(MSG.Message), "", MSG.Message.Replace("\n", vbCrLf))

            Dim StringMeasurements = Graphics.FromImage(My.Resources.UBUNTU)
            Dim TitleSize = StringMeasurements.MeasureString(s_Title, Renderer.TitleFont)
            Dim MessageSize = StringMeasurements.MeasureString(strMessage, Renderer.MessageFont)
            Dim Margins As Size = New Size(15, 5)
            Dim GraphicSize As Point = New Point With {
                .X = System.Math.Max(TitleSize.Width, MessageSize.Width) + Renderer.IconSize + Margins.Width * 3,
                .Y = Margins.Height * 2 + 5 + TitleSize.Height + MessageSize.Height + 25}

            If GraphicSize.Y < 80 Then
                GraphicSize.Y = 80
            End If

            Dim ImageValue As Bitmap = New Bitmap(GraphicSize.X, GraphicSize.Y + Renderer.YDelta)
            Using GraphicDevice As Graphics = Graphics.FromImage(ImageValue)

                GraphicDevice.CompositingQuality = Drawing2D.CompositingQuality.HighQuality

                '绘制表面纹理
                Call GraphicDevice.FillRegion(Brushes.Black, New Region(New Rectangle(New Point, GraphicSize)))
                '绘制消息内容
                GraphicDevice.DrawImage(MSG.Icon, 15, 15, Renderer.IconSize, Renderer.IconSize)
                GraphicDevice.DrawString(s_Title, Renderer.TitleFont, Brushes.WhiteSmoke, New Point(78, 15))
                GraphicDevice.DrawString(strMessage, Renderer.MessageFont, Brushes.White, New Point(78, 5 + 15 + TitleSize.Height))

                Return ImageValue
            End Using
        Catch ex As Exception
            FileIO.FileSystem.WriteAllText(My.Application.Info.DirectoryPath & "/notify_osd.log",
                                           Now.ToString & vbCrLf &
                                           "------------------------------------------------------------------------------------------" & vbCrLf &
                                           ex.ToString & vbCrLf, append:=True)
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' 假若使用默认的配置数据直接使用构造函数就可以了
    ''' </summary>
    Public Class MessageRender
        Public Property TitleFont As Font = New Font(FONT_FAMILY_MICROSOFT_YAHEI, 9, FontStyle.Bold)
        Public Property YDelta As Integer = 0
        Public Property MessageFont As Font = New Font(FONT_FAMILY_MICROSOFT_YAHEI, 8)
        Public Property IconSize As Integer = 48
    End Class
End Module
