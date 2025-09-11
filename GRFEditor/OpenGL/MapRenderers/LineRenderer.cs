﻿using GRFEditor.OpenGL.MapComponents;
using GRFEditor.OpenGL.WPF;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Matrix4 = OpenTK.Matrix4;

namespace GRFEditor.OpenGL.MapRenderers {
	public enum LinePointMode {
		Line,
		Point,
	}

	public class LineRenderer : Renderer {
		public Vector4 Color = new Vector4(0, 0, 0, 0.7f);
		private readonly float _width;
		private readonly LinePointMode _mode;
		private readonly Vector3[] _lines;
		private readonly RenderInfo _ri = new RenderInfo();

		public LineRenderer(Shader shader, float width, LinePointMode mode, params Vector3[] lines) {
			Shader = shader;
			_width = width;
			_mode = mode;
			_lines = lines;
		}

		public override void Load(OpenGLViewport viewport) {
			if (IsUnloaded)
				return;

			var vertices = new float[_lines.Length * 3];

			for (int i = 0; i < _lines.Length; i++) {
				for (int j = 0; j < 3; j++) {
					vertices[3 * i + j] = _lines[i][j];
				}
			}

			_ri.CreateVao();
			_ri.Vbo = new Vbo();
			_ri.Vbo.SetData(vertices, BufferUsageHint.StaticDraw, 3);

			GL.EnableVertexAttribArray(0);
			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
			IsLoaded = true;
		}

		public override void Render(OpenGLViewport viewport) {
			if (IsUnloaded)
				return;

			if (!IsLoaded) {
				Load(viewport);
			}

			Shader.Use();
			Shader.SetVector4("color", Color);

			Shader.SetMatrix4("model", Matrix4.Identity);
			Shader.SetMatrix4("view", ref viewport.View);
			Shader.SetMatrix4("projection", ref viewport.Projection);

			if (_mode == LinePointMode.Point) {
				GL.PointSize(_width);
				GL.BindVertexArray(_ri.Vao);
				GL.DrawArrays(PrimitiveType.Points, 0, _lines.Length);
			}
			else {	
				GL.LineWidth(_width);
				GL.BindVertexArray(_ri.Vao);
				GL.DrawArrays(PrimitiveType.Lines, 0, _lines.Length);
				GL.LineWidth(1.0f);
			}
		}

		public override void Unload() {
			IsUnloaded = true;
			_ri.Unload();
		}
	}
}
