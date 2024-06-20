using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;

using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.ADF;
using ESRI.ArcGIS.SystemUI;

using OutlookBar;
namespace BuildingGen {
  public sealed partial class MainForm : Form {
    #region class private members
    private GApplication app;
    private IMapControl3 m_mapControl = null;
    private string m_mapDocumentName = string.Empty;
    #endregion

    #region class constructor
    public MainForm() {
      InitializeComponent();
      this.ActiveControl = axMapControl1;
    }
    #endregion

    private void MainForm_Load(object sender, EventArgs e) {
      //get the MapControl
      m_mapControl = (IMapControl3)axMapControl1.Object;

      (axTOCControl1.Object as ITOCControl2).EnableLayerDragDrop = true;

      //axToolbarControl1.OperationStack = editToolBar.OperationStack;
      //axToolbarControl2.OperationStack = editToolBar.OperationStack;
      ICommandPool pool = new CommandPoolClass();
      editToolBar.CommandPool = pool;
      axToolbarControl1.CommandPool = pool;
      axToolbarControl2.CommandPool = pool;
      axToolbarControl3.CommandPool = pool;

      editToolBar.AddItem("esriControls.ControlsEditingEditorMenu", 0, -1, false, 0, esriCommandStyles.esriCommandStyleIconOnly);
      editToolBar.AddItem("esriControls.ControlsEditingEditTool", 0, -1, false, 0, esriCommandStyles.esriCommandStyleIconOnly);
      editToolBar.AddItem("esriControls.ControlsEditingSketchTool", 0, -1, false, 0, esriCommandStyles.esriCommandStyleIconOnly);
      editToolBar.AddItem("esriControls.ControlsEditingTargetToolControl", 0, -1, true, 0, esriCommandStyles.esriCommandStyleIconOnly);
      editToolBar.AddItem("esriControls.ControlsEditingTaskToolControl", 0, -1, true, 0, esriCommandStyles.esriCommandStyleIconOnly);

      //editToolBar.Visible = false;

      app = new GApplication(axMapControl1, axTOCControl1, this);
      //AddData ad = new AddData();
      //ad.OnGenCreate(app);
      //axToolbarControl2.AddItem(ad,-1,-1,false,0,esriCommandStyles.esriCommandStyleIconAndText);
      //GetCommand(Assembly.GetExecutingAssembly());

      axMapControl1.Focus();
      //axMapControl1.LostFocus += new EventHandler(axMapControl1_LostFocus);
      axMapControl1.Leave += new EventHandler(axMapControl1_Leave);
      axMapControl1.OnExtentUpdated += new IMapControlEvents2_Ax_OnExtentUpdatedEventHandler(axMapControl1_OnExtentUpdated);
      OutLookBar();
    }

    void axMapControl1_OnExtentUpdated(object sender, IMapControlEvents2_OnExtentUpdatedEvent e) {
      statusbarScale.Text = string.Format("�����ߣ� 1:{0}", axMapControl1.MapScale.ToString("#######.##"));
    }

    void axMapControl1_Leave(object sender, EventArgs e) {
      axMapControl1.Focus();
    }

    private void OutLookBar() {
      GridPanel gridepanel = new GridPanel(app.MapControl);
      outlookBar1.AddBand("����׼��", gridepanel);

      CreateNewDocument newDoc = new CreateNewDocument();
      newDoc.OnGenCreate(app);
      gridepanel.AddCommand(newDoc);
      menuNewDoc.Click += (sender, e) => { newDoc.OnClick(); };

      OpenDoc openDoc = new OpenDoc();
      openDoc.OnGenCreate(app);
      gridepanel.AddCommand(openDoc);
      menuOpenDoc.Click += (sender, e) => { openDoc.OnClick(); };

      SaveDoc saveDoc = new SaveDoc();
      saveDoc.OnGenCreate(app);
      gridepanel.AddCommand(saveDoc);
      menuSaveDoc.Click += (sender, e) => { saveDoc.OnClick(); };

      CloseDoc close = new CloseDoc();
      close.OnGenCreate(app);
      gridepanel.AddCommand(close);
      menuCloseDoc.Click += (sender, e) => { close.OnClick(); };

      ScaleOrg scaleOrg = new ScaleOrg();
      scaleOrg.OnGenCreate(app);
      gridepanel.AddCommand(scaleOrg);
      menuScaleOrg.Click += (sender, e) => { scaleOrg.OnClick(); };

      ScaleGen scaleGen = new ScaleGen();
      scaleGen.OnGenCreate(app);
      gridepanel.AddCommand(scaleGen);
      menuScaleGen.Click += (sender, e) => { scaleGen.OnClick(); };

      ShowGrid showGrid = new ShowGrid();
      showGrid.OnGenCreate(app);
      gridepanel.AddCommand(showGrid);

      ShowCircle showCircle = new ShowCircle();
      showCircle.OnGenCreate(app);
      gridepanel.AddCommand(showCircle);

      AddData addData = new AddData();
      addData.OnGenCreate(app);
      gridepanel.AddCommand(addData);

      BookMark bookmark = new BookMark();
      bookmark.OnGenCreate(app);
      gridepanel.AddCommand(bookmark);

      ClearWorkspace clw = new ClearWorkspace();
      clw.OnGenCreate(app);
      gridepanel.AddCommand(clw);

      gridepanel = new GridPanel(app.MapControl);
      outlookBar1.AddBand("��·�ۺ�", gridepanel);
      //��ͨ�Լ��
      RoadCheck roadCheck = new RoadCheck();
      roadCheck.OnGenCreate(app);
      gridepanel.AddCommand(roadCheck);
      //��·���
      RoadArea roadArea = new RoadArea();
      roadArea.OnGenCreate(app);
      gridepanel.AddCommand(roadArea);
      //��·�ּ�
      RoadRank roadRank = new RoadRank();
      roadRank.OnGenCreate(app);
      gridepanel.AddCommand(roadRank);
      //�ֹ��ּ�
      RoadRankChange roadRankChange = new RoadRankChange();
      roadRankChange.OnGenCreate(app);
      gridepanel.AddCommand(roadRankChange);
      
      //�Զ�ѡȡ
      //RoadAutoSelect roadAutoSelect = new RoadAutoSelect();
      //roadAutoSelect.OnGenCreate(app);
      //gridepanel.AddCommand(roadAutoSelect);
      //�ֶ�ѡȡ
      //RoadStroke roadStroke = new RoadStroke();
      //roadStroke.OnGenCreate(app);
      //gridepanel.AddCommand(roadStroke);

      //ͬ����·����
      ConnectRoad roadConnect = new ConnectRoad();
      roadConnect.OnGenCreate(app);
      gridepanel.AddCommand(roadConnect);

      //��ȡԭʼ����
      GetOriFeatsforRoad getOriFeats_road = new GetOriFeatsforRoad();
      getOriFeats_road.OnGenCreate(app);
      gridepanel.AddCommand(getOriFeats_road);

      //�ڵ�༭
      lineNodeEdit nodeE = new lineNodeEdit();
      nodeE.OnGenCreate(app);
      gridepanel.AddCommand(nodeE);

      //RoadTopoConnect roadTopoConnect = new RoadTopoConnect();
      //roadTopoConnect.OnGenCreate(app);
      //gridepanel.AddCommand(roadTopoConnect);

      //��·��ɢ
      RoadSplit roadSplit = new RoadSplit();
      roadSplit.OnGenCreate(app);
      gridepanel.AddCommand(roadSplit);

      //�����ߴ���
      RoadCheckEx roadCheckEx = new RoadCheckEx();
      roadCheckEx.OnGenCreate(app);
      gridepanel.AddCommand(roadCheckEx);

      //��������·�ּ�
      //RoadSelectEx roadselectEx = new RoadSelectEx();
      //roadselectEx.OnGenCreate(app);
      //gridepanel.AddCommand(roadselectEx);

      //ȥ��α�ڵ�
      ConnectRoadEx connectRoadEx = new ConnectRoadEx();
      connectRoadEx.OnGenCreate(app);
      gridepanel.AddCommand(connectRoadEx);

      gridepanel = new GridPanel(app.MapControl);
      outlookBar1.AddBand("�������ۺ�", gridepanel);

      //��������
      BuildingCheck buildingCheck = new BuildingCheck();
      buildingCheck.OnGenCreate(app);
      gridepanel.AddCommand(buildingCheck);

      //�������Զ�����
      BuildingD buidingd = new BuildingD();
      buidingd.OnGenCreate(app);
      gridepanel.AddCommand(buidingd);

      //���������
      BuildingB buiding = new BuildingB();
      buiding.OnGenCreate(app);
      gridepanel.AddCommand(buiding);

      //�����ﻯ��
      BuildingSimplifyGP buildSimp = new BuildingSimplifyGP();
      buildSimp.OnGenCreate(app);
      gridepanel.AddCommand(buildSimp);

      //������ȿ�
      BuildingC buildingC = new BuildingC();
      buildingC.OnGenCreate(app);
      gridepanel.AddCommand(buildingC);
      //BuildingC buidingc = new BuildingC();
      //buidingc.OnGenCreate(app);
      //gridepanel.AddCommand(buidingc);

      //��������ȡԭҪ��
      GetOriFeats getOriFeats_building = new GetOriFeats(GCityLayerType.������);
      getOriFeats_building.OnGenCreate(app);
      gridepanel.AddCommand(getOriFeats_building);

      //�ڵ�༭
      NodeEditorEx nodeBuilding = new NodeEditorEx(GCityLayerType.������);
      nodeBuilding.OnGenCreate(app);
      gridepanel.AddCommand(nodeBuilding);

      //����
      Reshape reshapeBuild = new Reshape(GCityLayerType.������);
      reshapeBuild.OnGenCreate(app);
      gridepanel.AddCommand(reshapeBuild);

      //���������
      //SimplifyErrCheck sErrCheck_build = new SimplifyErrCheck(GCityLayerType.������);
      //sErrCheck_build.OnGenCreate(app);
      //gridepanel.AddCommand(sErrCheck_build);

      //���ݵ�·����
      BuildingGroupByRoad bywh = new BuildingGroupByRoad();
      bywh.OnGenCreate(app);
      gridepanel.AddCommand(bywh);

      BuildingSelect buildingselect = new BuildingSelect();
      buildingselect.OnGenCreate(app);
      gridepanel.AddCommand(buildingselect);

      BuildingAdd buidingadd = new BuildingAdd();
      buidingadd.OnGenCreate(app);
      gridepanel.AddCommand(buidingadd);

      //��ͻ����
      ConflictOfBuildingAndRoad conflict = new ConflictOfBuildingAndRoad();
      conflict.OnGenCreate(app);
      gridepanel.AddCommand(conflict);

      //�и���
      //BuildCut ccc = new BuildCut();
      //ccc.OnGenCreate(app);
      //gridepanel.AddCommand(ccc);

      gridepanel = new GridPanel(app.MapControl);
      outlookBar1.AddBand("ˮϵ�ۺ�", gridepanel);

      //WaterCheck waterCheck = new WaterCheck();
      //waterCheck.OnGenCreate(app);
      //gridepanel.AddCommand(waterCheck);

      //WaterAggre waterAggre = new WaterAggre();
      //waterAggre.OnGenCreate(app);
      //gridepanel.AddCommand(waterAggre);

      //��������
      DitchAreaToLine DitchToLine = new DitchAreaToLine();
      DitchToLine.OnGenCreate(app);
      gridepanel.AddCommand(DitchToLine);

      /*CutForDitches reshapeDitches = new CutForDitches();
      reshapeDitches.OnGenCreate(app);
      gridepanel.AddCommand(reshapeDitches);*/

      //�������ữ
      CentralizeNarrowRiver sssss = new CentralizeNarrowRiver();
      sssss.OnGenCreate(app);
      gridepanel.AddCommand(sssss);

      //����ѡȡ����
      DitchesAutoSelectEx selectEx = new DitchesAutoSelectEx();
      selectEx.OnGenCreate(app);
      gridepanel.AddCommand(selectEx);

      SelectDitchByHands reshapeDitches2 = new SelectDitchByHands();
      reshapeDitches2.OnGenCreate(app);
      gridepanel.AddCommand(reshapeDitches2);

      WaterD waterD = new WaterD();
      waterD.OnGenCreate(app);
      gridepanel.AddCommand(waterD);

      WaterB waterB = new WaterB();
      waterB.OnGenCreate(app);
      gridepanel.AddCommand(waterB);

      WaterSimplify2 waterSimplify = new WaterSimplify2();
      waterSimplify.OnGenCreate(app);
      gridepanel.AddCommand(waterSimplify);

      /*WaterLineSplit wls = new WaterLineSplit();
      wls.OnGenCreate(app);
      gridepanel.AddCommand(wls);*/

      WaterLineHandle wlh = new WaterLineHandle();
      wlh.OnGenCreate(app);
      gridepanel.AddCommand(wlh);

      WaterLineConnect connect = new WaterLineConnect();
      connect.OnGenCreate(app);
      gridepanel.AddCommand(connect);

      //RiverSimplify riverSimplify = new RiverSimplify();
      //riverSimplify.OnGenCreate(app);
      //gridepanel.AddCommand(riverSimplify);

      //SimplifyErrCheck sErrCheck_Water = new SimplifyErrCheck(GCityLayerType.ˮϵ);
      //sErrCheck_Water.OnGenCreate(app);
      //gridepanel.AddCommand(sErrCheck_Water);

      //DitchGroup selectDit2 = new DitchGroup();
      //selectDit2.OnGenCreate(app);
      //gridepanel.AddCommand(selectDit2);


      //SelectSomeDitches s = new SelectSomeDitches();
      //s.OnGenCreate(app);
      //gridepanel.AddCommand(s);

      //SelectPoolsByArea ss = new SelectPoolsByArea();
      //ss.OnGenCreate(app);
      //gridepanel.AddCommand(ss);

      //UnionForSelectLongDitches sss = new UnionForSelectLongDitches();
      //sss.OnGenCreate(app);
      //gridepanel.AddCommand(sss);

      //CutRiverToPieces ssss = new CutRiverToPieces();
      //ssss.OnGenCreate(app);
      //gridepanel.AddCommand(ssss);

      WaterAggreforSelect waterAs = new WaterAggreforSelect();
      waterAs.OnGenCreate(app);
      gridepanel.AddCommand(waterAs);

      GetOriFeats getOriFeats_water = new GetOriFeats(GCityLayerType.ˮϵ);
      getOriFeats_water.OnGenCreate(app);
      gridepanel.AddCommand(getOriFeats_water);

      NodeEditorEx nodeWater = new NodeEditorEx(GCityLayerType.ˮϵ);
      nodeWater.OnGenCreate(app);
      gridepanel.AddCommand(nodeWater);

      Reshape reshapeForWater = new Reshape(GCityLayerType.ˮϵ);
      reshapeForWater.OnGenCreate(app);
      gridepanel.AddCommand(reshapeForWater);

      HydroConflict hydroConflict = new HydroConflict();
      hydroConflict.OnGenCreate(app);
      gridepanel.AddCommand(hydroConflict);

      gridepanel = new GridPanel(app.MapControl);
      outlookBar1.AddBand("ֲ���ۺ�", gridepanel);

      PlantBuffer pb = new PlantBuffer();
      pb.OnGenCreate(app);
      gridepanel.AddCommand(pb);

      VegetationGeneralizeEx vGenEx = new VegetationGeneralizeEx();
      vGenEx.OnGenCreate(app);
      gridepanel.AddCommand(vGenEx);

      GetOriFeats getOriFeats_vetetation = new GetOriFeats(GCityLayerType.ֲ��);
      getOriFeats_vetetation.OnGenCreate(app);
      gridepanel.AddCommand(getOriFeats_vetetation);

      NodeEditorEx nodePlant = new NodeEditorEx(GCityLayerType.ֲ��);
      nodePlant.OnGenCreate(app);
      gridepanel.AddCommand(nodePlant);

      Reshape reshapeForPlant = new Reshape(GCityLayerType.ֲ��);
      reshapeForPlant.OnGenCreate(app);
      gridepanel.AddCommand(reshapeForPlant);

      SimplifyErrCheck sErrCheck_Plant = new SimplifyErrCheck(GCityLayerType.ֲ��);
      sErrCheck_Plant.OnGenCreate(app);
      gridepanel.AddCommand(sErrCheck_Plant);

      gridepanel = new GridPanel(app.MapControl);
      outlookBar1.AddBand("����Ҫ���ۺ�", gridepanel);
      POISelection poi = new POISelection();
      poi.OnGenCreate(app);
      gridepanel.AddCommand(poi);

      AnnoSelect annoSelect = new AnnoSelect();
      annoSelect.OnGenCreate(app);
      gridepanel.AddCommand(annoSelect);


      gridepanel = new GridPanel(app.MapControl);
      outlookBar1.AddBand("������", gridepanel);
      StreetBlock streetBlock = new StreetBlock();
      streetBlock.OnGenCreate(app);
      gridepanel.AddCommand(streetBlock);

      outlookBar1.SelectedBand = 0;
    }
    private void AddIcon(IconPanel contentPanel, ESRI.ArcGIS.SystemUI.ICommand command, Image image) {
      EventHandler handler = (sender, arg) => {
        if (command is ESRI.ArcGIS.SystemUI.ITool) {
          axMapControl1.CurrentTool = command as ITool;
          command.OnClick();
        }
        else {
          command.OnClick();
        }
      };
      contentPanel.AddIcon(command.Caption, image, handler);
    }
    private void GetCommand(Assembly assembly) {
      Type[] types = assembly.GetExportedTypes();
      foreach (Type t in types) {
        if (t.IsClass && t.IsPublic && !t.IsAbstract) {
          if (t.GetInterface("IGenCreate") != null) {
            IGenCreate tool = null;
            try {
              tool = Activator.CreateInstance(t) as IGenCreate;
            }
            catch {
              continue;
            }
            tool.OnGenCreate(this.app);
            if ((tool as ICommand).Category == "GSystem") {
              axToolbarControl2.AddItem(tool, -1, -1, false, 0, esriCommandStyles.esriCommandStyleIconAndText);
            }
            else {
              axToolbarControl3.AddItem(tool, -1, -1, false, 0, esriCommandStyles.esriCommandStyleIconAndText);
            }
            if (tool is ICommand && (tool as ICommand).Name == "SaveEdit") {
              ICommand cmd = new ESRI.ArcGIS.Controls.ControlsUndoCommandClass();
              axToolbarControl2.AddItem(cmd, -1, -1, false, 0, esriCommandStyles.esriCommandStyleIconOnly);
              cmd = new ESRI.ArcGIS.Controls.ControlsRedoCommandClass();
              axToolbarControl2.AddItem(cmd, -1, -1, false, 0, esriCommandStyles.esriCommandStyleIconOnly);

            }

          }
        }
      }
    }

    #region Main Menu event handlers
    private void menuNewDoc_Click(object sender, EventArgs e) {

    }

    private void menuOpenDoc_Click(object sender, EventArgs e) {

    }

    private void menuSaveDoc_Click(object sender, EventArgs e) {
      if (app.Workspace != null) {
        app.Workspace.Save();
      }
      else {
      }
    }

    private void menuSaveAs_Click(object sender, EventArgs e) {
    }

    private void menuExitApp_Click(object sender, EventArgs e) {

      Application.Exit();
    }
    #endregion

    //listen to MapReplaced evant in order to update the statusbar and the Save menu
    private void axMapControl1_OnMapReplaced(object sender, IMapControlEvents2_OnMapReplacedEvent e) {
      //get the current document name from the MapControl
      m_mapDocumentName = m_mapControl.DocumentFilename;

      //if there is no MapDocument, diable the Save menu and clear the statusbar
      if (m_mapDocumentName == string.Empty) {
        //menuSaveDoc.Enabled = false;
        statusBarXY.Text = string.Empty;
      }
      else {
        //enable the Save manu and write the doc name to the statusbar
        //menuSaveDoc.Enabled = true;
        statusBarXY.Text = Path.GetFileName(m_mapDocumentName);
      }
    }

    private void axMapControl1_OnMouseMove(object sender, IMapControlEvents2_OnMouseMoveEvent e) {
      statusBarXY.Width = 300;
      statusBarXY.TextAlign = ContentAlignment.MiddleLeft;
      statusBarXY.Text = string.Format("��ǰ���λ�ã� {0}, {1}  {2}", e.mapX.ToString("#######.##"), e.mapY.ToString("#######.##"), axMapControl1.MapUnits.ToString().Substring(4));
    }

    private void CloseToolStripMenuItem_Click(object sender, EventArgs e) {
      app.CloseWorkspace();
    }

    private void aboutToolStripMenuItem_Click(object sender, EventArgs e) {
      GAbout a = new GAbout();
      a.ShowDialog();
    }

    private void GenParaToolStripMenuItem_Click(object sender, EventArgs e) {
      app.GenParaDlg.Show();
    }

    private void ShowMap(object sender, EventArgs e) {
      if (app.Workspace == null)
        return;
      bool showGen = menuViewShowGen.Checked;
      bool showOrg = menuViewShowOrg.Checked;
      EnvRenderMode envMode = EnvRenderMode.һ����ʾ;
      if (showGen && showOrg) {
        envMode = EnvRenderMode.һ����ʾ;
      }
      else {
        envMode = showGen ? EnvRenderMode.����ʾ�ۺϲ� : EnvRenderMode.����ʾԭͼ��;
      }

      foreach (GLayerInfo info in app.Workspace.LayerManager.Layers) {
        LayerRenderMode layerMode = LayerRenderMode.��������ʾ;
        ToolStripMenuItem mi = null;
        switch (info.LayerType) {
        case GCityLayerType.������:
        mi = menuHighBuilding;
        break;
        case GCityLayerType.��·:
        mi = menuHightRoad;
        break;
        case GCityLayerType.ˮϵ:
        mi = menuHighWater;
        break;
        case GCityLayerType.ֲ��:
        mi = MenuHighPlant;
        break;
        case GCityLayerType.��·:
        mi = menuHightRoad;
        break;
        case GCityLayerType.�߼�:
        mi = menuHightRoad;
        break;
        case GCityLayerType.����:
        mi = menuHighBuilding;
        break;
        case GCityLayerType.����:
        mi = MenuHighPlant;
        break;
        case GCityLayerType.BRT��ͨ��:
        mi = menuHightRoad;
        break;
        case GCityLayerType.�̻���:
        mi = MenuHighPlant;
        break;
        default:
        break;
        }
        FeatureRenderMode fmode =
            (menuViewBuildingGroup.Checked ? FeatureRenderMode.ǿ��������ʾ : FeatureRenderMode.������ʾ)
            |
            (menuViewBuildingStruct.Checked ? FeatureRenderMode.ǿ�������ṹ��ʾ : FeatureRenderMode.������ʾ)
            |
            (menuViewRoadRank.Checked ? FeatureRenderMode.ǿ����·�ּ���ʾ : FeatureRenderMode.������ʾ)
            ;
        layerMode = mi.Checked ? LayerRenderMode.��׼����ʾ : LayerRenderMode.��������ʾ;
        CityRender.RenderLayer(info, envMode, layerMode, fmode);
      }

      app.MapControl.Refresh();
    }

    private void ����ToolStripMenuItem_Click(object sender, EventArgs e) {
      GAbout about = new GAbout();
      about.ShowDialog();
    }

    private void miWorkspaceInfo_Click(object sender, EventArgs e) {
      if (app.Workspace == null)
        return;
      GWorkSpaceInfoDlg dlg = new GWorkSpaceInfoDlg(app.Workspace);
      dlg.ShowDialog();
    }
  }
}