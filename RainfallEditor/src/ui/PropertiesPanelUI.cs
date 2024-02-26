using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static partial class EditorUI
{
	static Entity lastSelectedEntity = null;
	static byte[] renameBuffer = new byte[256];

	static unsafe void PropertiesPanel(EditorInstance instance, RainfallEditor editor)
	{
		ImGui.SetNextWindowPos(new Vector2(Display.width - PROPERTIES_PANEL_WIDTH, ImGui.GetFrameHeight() * 2));
		ImGui.SetNextWindowSize(new Vector2(PROPERTIES_PANEL_WIDTH, Display.height - ImGui.GetFrameHeight() * 2));
		if (ImGui.Begin("Properties", null, ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse))
		{
			if (instance.selectedNode != null)
			{
				if (instance.selectedNode != lastSelectedEntity)
				{
					StringUtils.WriteString(renameBuffer, instance.selectedNode.name);
					lastSelectedEntity = instance.selectedNode;
				}

				// Name
				ImGui.TextUnformatted("Name");
				ImGui.SameLine();
				ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().x);
				fixed (byte* renameBufferPtr = renameBuffer)
				{
					if (ImGui.InputText("##entity_name", renameBufferPtr, (ulong)renameBuffer.Length, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
					{
						instance.selectedNode.name = new string((sbyte*)renameBufferPtr);
					}
				}

				string getFilenameFromPath(string path)
				{
					int slash = path.LastIndexOfAny(new char[] { '/', '\\' });
					if (slash != -1)
						return path.Substring(slash + 1);
					return path;
				}

				// Model
				ImGui.TextUnformatted("Model");
				ImGui.SameLine();
				ImGui.InputText("##entity_model", instance.selectedNode.modelPath != null ? getFilenameFromPath(instance.selectedNode.modelPath) : "", ImGuiInputTextFlags.ReadOnly | ImGuiInputTextFlags.AutoSelectAll);
				ImGui.SameLine();
				if (ImGui.Button("Browse"))
				{
					byte* outPath = null;
					NFDResult result = NFD.NFD_OpenDialog(null, null, &outPath);
					if (result == NFDResult.NFD_OKAY)
					{
						instance.selectedNode.modelPath = new string((sbyte*)outPath);
						instance.selectedNode.reloadModel();
						NFD.NFDi_Free(outPath);
					}
				}
			}
		}
		ImGui.End();
	}
}
