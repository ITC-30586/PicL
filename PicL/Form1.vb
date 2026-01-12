'
'
'
Imports System.Management
Imports System.Reflection.Emit
Imports System.Windows.Forms.VisualStyles.VisualStyleElement
Imports C1.Framework.Extension
Imports C1.Util.Win.Win32
Imports C1.Win.C1FlexGrid
Imports C1.Win.TouchToolKit

Public Class Form1

    ' 定数定義
    Private Const GRID_SIZE As Integer = 100
    Private Const CELL_HEIGHT As Integer = 9
    Private Const CELL_WIDTH As Integer = 15
    Private Const APP_VERSION As String = "Ver1.0.0"

    ' フィールド
    Private stCurrentDir As String
    Private styleNS1 As CellStyle
    Private styleNS2 As CellStyle
    Private styleNS3 As CellStyle
    Private styleNS4 As CellStyle

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            ' 2重起動防止
            If IsApplicationAlreadyRunning() Then
                Me.Close()
                Return
            End If

            ' 起動パス
            stCurrentDir = System.IO.Directory.GetCurrentDirectory()

            ' 初期化処理
            InitializeZoomPanel()
            Call Init_Form1()
            Call Init_C1FG01()
            Call InitializeCellStyles()

        Catch ex As Exception
            ErrLOG("Form1_Load", ex.Message)
            MessageBox.Show("初期化中にエラーが発生しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' 2重起動チェック
    Private Function IsApplicationAlreadyRunning() As Boolean
        Dim pn As String = Process.GetCurrentProcess.ProcessName
        Return Process.GetProcessesByName(pn).GetUpperBound(0) > 0
    End Function

    ' ZoomPanelの初期化
    Private Sub InitializeZoomPanel()
        With C1ZoomPanel1
            .ZoomFactor = 1.0F
            .MaxZoomFactor = 4.0F
            .AllowDoubleTapZoom = True
            .ZoomSnapDistance = 0.05F
            .ZoomSnapPoints.Add(2.0F)
            .ZoomSnapPoints.Add(3.0F)
            .ZoomSnapPoints.Add(4.0F)
            .InnerPanelLayoutMode = InnerPanelLayoutMode.MiddleCenter
            .BoundaryFeedbackMode = BoundaryFeedbackMode.Standard
        End With
    End Sub

    ' セルスタイルの初期化
    Private Sub InitializeCellStyles()
        ' 作業A用スタイル
        styleNS1 = C1FlexGrid1.Styles.Add("NS1")
        With styleNS1
            .BackColor = System.Drawing.Color.FromArgb(50, 169, 169, 169)
            .ForeColor = System.Drawing.Color.Crimson
            .Font = New Font("ＭＳ 明朝", 6, FontStyle.Regular)
            .TextAlign = TextAlignEnum.CenterCenter
        End With

        ' 作業B用スタイル
        styleNS2 = C1FlexGrid1.Styles.Add("NS2")
        With styleNS2
            .BackColor = System.Drawing.Color.FromArgb(50, 210, 180, 140)
            .ForeColor = System.Drawing.Color.DodgerBlue
            .Font = New Font("ＭＳ 明朝", 6, FontStyle.Regular)
            .TextAlign = TextAlignEnum.CenterCenter
        End With

        ' 作業C用スタイル
        styleNS3 = C1FlexGrid1.Styles.Add("NS3")
        With styleNS3
            .BackColor = System.Drawing.Color.FromArgb(50, 135, 206, 235)
            .ForeColor = System.Drawing.Color.Green
            .Font = New Font("ＭＳ 明朝", 6, FontStyle.Regular)
            .TextAlign = TextAlignEnum.CenterCenter
        End With

        ' 作業混合用スタイル
        styleNS4 = C1FlexGrid1.Styles.Add("NS4")
        With styleNS4
            .BackColor = System.Drawing.Color.FromArgb(50, 255, 127, 80)
            .ForeColor = System.Drawing.Color.SaddleBrown
            .Font = New Font("ＭＳ 明朝", 6, FontStyle.Regular)
            .TextAlign = TextAlignEnum.CenterCenter
        End With
    End Sub

    ' エラーログを書き出す
    Public Sub ErrLOG(ByVal C1 As String, ByVal C2 As String)
        Dim FileName As String
        Dim WS As System.IO.StreamWriter

        Try
            FileName = stCurrentDir & "\errlog.txt"
            WS = System.IO.File.AppendText(FileName)
            WS.WriteLine($"{C1}:{C2}:{Format(Now, "yyyy/MM/dd HH:mm:ss")}")
            WS.Close()
        Catch ex As Exception
            Debug.WriteLine($"エラーログ書き込み失敗: {ex.Message}")
        End Try
    End Sub

    ' 起動時画面設定
    Private Sub Init_Form1()
        Me.Text = $"PICロケーション作業記録 {APP_VERSION}"
    End Sub

    Private Sub Init_C1FG01()
        Try
            C1FlexGrid1.Visible = False
            C1FlexGrid1.Clear()

            ' 背景画像設定
            Dim imagePath As String = System.Windows.Forms.Application.StartupPath & "\Sample.png"
            If System.IO.File.Exists(imagePath) Then
                C1FlexGrid1.BackgroundImage = Image.FromFile(imagePath)
                C1FlexGrid1.BackgroundImageLayout = ImageLayout.Stretch
            End If

            ' 罫線設定
            With C1FlexGrid1.Styles.Normal.Border
                .Color = System.Drawing.Color.Gray
                .Style = BorderStyleEnum.Dotted
                .Width = 1
            End With

            ' フォーカス罫線設定
            With C1FlexGrid1.Styles.Focus.Border
                .Color = System.Drawing.Color.Red
                .Style = BorderStyleEnum.Double
            End With

            ' グリッド設定
            With C1FlexGrid1
                .Rows.DefaultSize = CELL_HEIGHT
                .Cols.DefaultSize = CELL_WIDTH
                .Rows.Fixed = 0
                .Cols.Fixed = 0
                .Cols.Count = GRID_SIZE + 1
                .Rows.Count = GRID_SIZE + 1
                .ScrollBars = ScrollBars.None

                For i As Integer = 0 To GRID_SIZE
                    .Cols(i).Width = CELL_WIDTH
                    .Rows(i).Height = CELL_HEIGHT
                Next
            End With

            C1FlexGrid1.Visible = True
            C1FlexGrid1.Select(-1, -1, True)

        Catch ex As Exception
            ErrLOG("Init_C1FG01", ex.Message)
            Throw
        End Try
    End Sub

    ' レコード選択
    Private Sub C1FlexGrid1_Click(sender As Object, e As EventArgs)
        Try
            Dim x As Integer = C1FlexGrid1.Col
            Dim y As Integer = C1FlexGrid1.Row

            ' 無効な選択をチェック
            If y <= 0 OrElse x < 0 Then
                Exit Sub
            End If

            ' セルにデータを設定
            C1FlexGrid1.SetCellStyle(y, x, styleNS4)
            C1FlexGrid1.SetData(y, x, "●")

            ' 作業位置を表示
            TextBox1.Text = $"({x},{y})"

            ' カレントセルのカーソルを非表示
            C1FlexGrid1.Row = -1
            C1FlexGrid1.Col = -1

        Catch ex As Exception
            ErrLOG("C1FlexGrid1_Click", ex.Message)
            MessageBox.Show("セル選択中にエラーが発生しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs)
        C1ZoomPanel1.ZoomFactor = 2.0F
    End Sub

End Class


