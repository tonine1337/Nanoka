import React from 'react';
import Dropzone from 'react-dropzone';

const DropzoneStyle = {
  flex: 1,
  display: 'flex',
  flexDirection: 'column',
  alignItems: 'center',
  padding: '5rem',
  borderWidth: 2,
  borderRadius: 10,
  borderColor: '#eeeeee',
  borderStyle: 'dashed',
  backgroundColor: '#fafafa',
  color: '#bdbdbd',
  outline: 'none'
};

export default DropzoneStyle;

export function createDropzone() {
  return (
    <Dropzone onDrop={f => this.setState({ file: f[0] })} accept="application/x-zip-compressed">
      {({ getRootProps, getInputProps }) => (
        <div {...getRootProps()} style={DropzoneStyle}>
          <input {...getInputProps()} />
          {this.state.file
            ? (
              <div style={{ textAlign: 'center' }}>
                <strong>{this.state.file.name}</strong>
                <br />
                <span>{(this.state.file.size / 1000000).toFixed(1)} KB</span>
              </div>
            ) : (
              <div>Click here to select a file, or drag-drop it.</div>
            )}
        </div>
      )}
    </Dropzone>
  );
}
