﻿using GRF.FileFormats.RswFormat.RswObjects;
using GRFEditor.OpenGL.MapComponents;
using GRFEditor.OpenGL.WPF;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GRFEditor.OpenGL.MapRenderers {
	public class ModelRenderer : Renderer {
		public readonly SharedRsmRenderer Renderer;
		public readonly Model Model;
		public Matrix4 MatrixCache;
		public bool IsHidden { get; set; }
		public bool IsMatrixCached { get; set; }
		public bool ReverseCullFace { get; set; }

		public ModelRenderer(Shader shader, Model model, SharedRsmRenderer renderer) {
			Shader = shader;
			Model = model;
			Renderer = renderer;
		}

		public void CalculateCachedMatrix() {
			if (IsMatrixCached)
				return;

			MatrixCache = Matrix4.Identity;
			MatrixCache = GLHelper.Scale(MatrixCache, new Vector3(1, 1, -1));

			if (Renderer.Gnd != null) {
				MatrixCache = GLHelper.Translate(MatrixCache, new Vector3(5 * Renderer.Gnd.Width + Model.Position.X, -Model.Position.Y, -10 - 5 * Renderer.Gnd.Height + Model.Position.Z));
				MatrixCache = GLHelper.Rotate(MatrixCache, -GLHelper.ToRad(Model.Rotation.Z), new Vector3(0, 0, 1));
				MatrixCache = GLHelper.Rotate(MatrixCache, -GLHelper.ToRad(Model.Rotation.X), new Vector3(1, 0, 0));
				MatrixCache = GLHelper.Rotate(MatrixCache, GLHelper.ToRad(Model.Rotation.Y), new Vector3(0, 1, 0));
				MatrixCache = GLHelper.Scale(MatrixCache, new Vector3(Model.Scale.X, -Model.Scale.Y, Model.Scale.Z));

				if (Renderer.Rsm.Version < 2.2) {
					MatrixCache = GLHelper.Translate(MatrixCache, new Vector3(-Renderer.Rsm.RealBox.Center.X, Renderer.Rsm.RealBox.Min.Y, -Renderer.Rsm.RealBox.Center.Z));
				}
				else {
					MatrixCache = GLHelper.Scale(MatrixCache, new Vector3(1, -1, 1));
				}

				if (MatrixCache[3, 0] * 0.2f < 0 ||
					MatrixCache[3, 0] * 0.2f > Renderer.Gnd.Header.Width * 2 ||
					MatrixCache[3, 2] * 0.2f < 0 ||
					MatrixCache[3, 2] * 0.2f > Renderer.Gnd.Header.Height * 2) {
					IsHidden = true;
				}
			}
			else {
				if (Renderer.Rsm.Version < 2.2) {
					MatrixCache = GLHelper.Translate(MatrixCache, -Renderer.Rsm.DrawnBox.Center);
					MatrixCache = GLHelper.Scale(MatrixCache, new Vector3(1, -1, 1));
				}
				else {
					RsmBoundingBox box = new RsmBoundingBox();
					var vs = Renderer.Rsm.MainMesh.GetAllDrawnVertices(Matrix4.Identity);
					foreach (var v in vs) {
						box.AddVector(v);
					}
					MatrixCache = GLHelper.Translate(MatrixCache, Renderer.Rsm.DrawnBox.Center * new Vector3(1, -1, -1));
					MatrixCache = GLHelper.Scale(MatrixCache, new Vector3(-1, 1, 1));
				}
			}

			if (Model != null && (Model.Scale.X * Model.Scale.Y * Model.Scale.Z * (Renderer.Rsm.Version >= 2.2 ? -1 : 1) < 0))
				ReverseCullFace = true;

			IsMatrixCached = true;
		}

		public ModelRenderer(RendererLoadRequest request, Rsm rsm, Shader shader) {
			Shader = shader;
			Renderer = new SharedRsmRenderer(request, shader, rsm);
		}

		public override void Load(OpenGLViewport viewport) {
			if (IsUnloaded)
				return;

			CalculateCachedMatrix();
			Renderer.Load(viewport);
			IsLoaded = true;
		}

		public override void Render(OpenGLViewport viewport) {
			if (IsUnloaded || IsHidden)
				return;
			if (!IsLoaded)
				Load(viewport);

			if (viewport.RenderOptions.ShowWireframeView)
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

			if (viewport.RenderOptions.ShowPointView)
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Point);

			if (viewport.RenderOptions.ShowWireframeView || viewport.RenderOptions.ShowPointView) {
				Shader.SetBool("fixedColor", true);
			}
			else {
				Shader.SetBool("fixedColor", false);
			}

			Renderer.Render(viewport, ref MatrixCache);

			if (viewport.RenderOptions.ShowWireframeView)
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

			if (viewport.RenderOptions.ShowPointView)
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
		}

		public override void Unload() {
			IsUnloaded = true;
			Renderer.Unload();
		}
	}
}
