using System;
using System.Windows.Forms.VisualStyles;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using Ninject;
using SharpFlame.Core;
using SharpFlame.UiOptions;
using Z.ExtensionMethods.Object;

namespace SharpFlame.Gui.Sections
{
	public static class HeightPresetGroup
	{
		public const string Left = "LEFT";
		public const string Right = "RIGHT";
	}


	public class HeightTab : TabPage
	{
        [Inject]
        internal Options UiOptions { get; set; }

        protected NumericUpDown numRadius;

		protected Button cmdCircularTool;
		protected Button cmdSquareTool;
		protected DropDown ddlMode;
		protected ImageView imgLeftClick;
		protected ImageView imgRightClick;
		protected NumericUpDown numIncrDecr;
		protected Panel panSmooth;
		protected Panel panSetHeight;
		protected Panel panIncrDecr;
		protected NumericUpDown numLeftClick;
		protected NumericUpDown numRightClick;
		protected TableLayout tblLeftPresets;
		protected TableLayout tblRightPresets;
		protected CheckBox chkIncrDecrFade;
		protected NumericUpDown numSmoothRate;

        public HeightTab ()
        {
	        XomlReader.Load(this);

	        this.cmdSquareTool.Image = Resources.Square;
	        this.cmdCircularTool.Image = Resources.Circle;
	        this.imgLeftClick.Image = Resources.MouseLeft;
	        this.imgRightClick.Image = Resources.MouseRight;

			this.panSetHeight.Bind(x => x.Visible, this.ddlMode.SelectedIndexBinding.Convert(i => i == 0));
			this.panIncrDecr.Bind(x => x.Visible, this.ddlMode.SelectedIndexBinding.Convert(i => i == 1));
			this.panSmooth.Bind(x => x.Visible, this.ddlMode.SelectedIndexBinding.Convert(i => i == 2));

	        this.numRadius.MaxValue = Constants.MapMaxSize;
	        this.numRadius.Increment = 0.5;
	        this.numRadius.ValueBinding.Bind(UiOptions.Height.Brush, b => b.Radius);

	        this.numLeftClick.ValueBinding.Bind(UiOptions.Height, h => h.LmbHeight);
	        this.numRightClick.ValueBinding.Bind(UiOptions.Height, h => h.RmbHeight);

	        this.numIncrDecr.ValueBinding.Bind(UiOptions.Height, h => h.ChangeRate);
	        this.chkIncrDecrFade.CheckedBinding.Bind(UiOptions.Height, h => h.ChangeFade);

	        this.numSmoothRate.ValueBinding.Bind(UiOptions.Height, h => h.SmoothRate);
        }


		void AnyPreset_PreLoad(object sender, EventArgs e)
		{
			var preset = sender as Button;
			if( preset == null ) return;

			NumericUpDown upDownContext = null;
			TableLayout presetGroup = null;
			if( preset.Tag.To<string>() == HeightPresetGroup.Left )
			{
				upDownContext = this.numLeftClick;
				presetGroup = this.tblLeftPresets;
			}
			else if( preset.Tag.To<string>() == HeightPresetGroup.Right )
			{
				upDownContext = this.numRightClick;
				presetGroup = this.tblRightPresets;
			}

			preset.Bind(b => b.Text, upDownContext.ValueBinding.Convert(d =>
			{
				if( preset.Enabled == false )
				{
					return d.ToString();
				}
				return preset.Text;

			}), DualBindingMode.OneWay);

			preset.Bind(b => b.Enabled,
				Binding.Delegate(() => presetGroup.DataContext != preset,
				addChangeEvent: handlerToExecuteWhenSourceChanges => presetGroup.DataContextChanged += handlerToExecuteWhenSourceChanges,
				removeChangeEvent: handlerToExecuteWhenSourceChanges => presetGroup.DataContextChanged += handlerToExecuteWhenSourceChanges));

			preset.Bind(b => b.BackgroundColor, Binding.Delegate(() =>
			{
				if( presetGroup.DataContext == preset )
				{
					return Eto.Drawing.Colors.SkyBlue;
				}
				return Eto.Drawing.Colors.Transparent;
			},
				addChangeEvent: h => presetGroup.DataContextChanged += h,
				removeChangeEvent: h => presetGroup.DataContextChanged += h));
		}

		protected void AnyPreset_Click(object sender, EventArgs e)
		{
			var preset = sender as Button;
			if( preset == null ) return;

			var value = Convert.ToInt32(preset.Text);

			if( preset.Tag.To<string>() == HeightPresetGroup.Left )
			{

				this.tblLeftPresets.DataContext = preset;
				this.numLeftClick.Value = value;
			}
			else if( preset.Tag.To<string>() == HeightPresetGroup.Right )
			{
				this.tblRightPresets.DataContext = preset;
				this.numRightClick.Value = value;
			}
		}

		void ToolSelection_Click(object sender, EventArgs e)
		{
			if( sender == this.cmdCircularTool )
			{
				this.cmdCircularTool.Enabled = false;
				this.cmdSquareTool.Enabled = true;
				this.cmdCircularTool.BackgroundColor = Eto.Drawing.Colors.Coral;
				this.cmdSquareTool.BackgroundColor = Eto.Drawing.Colors.Transparent;
				this.UiOptions.Height.Brush.Shape = ShapeType.Circle;
				
			}
			else if( sender == this.cmdSquareTool )
			{
				this.cmdCircularTool.Enabled = true;
				this.cmdSquareTool.Enabled = false;
				this.cmdCircularTool.BackgroundColor = Eto.Drawing.Colors.Transparent;
				this.cmdSquareTool.BackgroundColor = Eto.Drawing.Colors.Coral; 
				this.UiOptions.Height.Brush.Shape = ShapeType.Square;
			}
		}

		void ddlMode_SelectedIndexChanged(object sender, EventArgs e)
		{
			if( this.ddlMode.SelectedIndex == 0 )
			{
				UiOptions.MouseTool = MouseTool.HeightSetBrush;
			}
			else if( this.ddlMode.SelectedIndex == 1 )
			{
				UiOptions.MouseTool = MouseTool.HeightChangeBrush;
			}
			else if( this.ddlMode.SelectedIndex == 2 )
			{
				UiOptions.MouseTool = MouseTool.HeightSmoothBrush;
			}
		}
	}
}

