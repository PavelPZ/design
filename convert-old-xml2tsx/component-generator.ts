import * as fs from 'fs';
import * as _ from 'lodash';

interface IComps {
  root: IComp;
  types: { [type: string]: IComp; };
}

interface IComp {
  typ: string;
  anc: string;
  descs: Array<IComp>;
}

const ignores = {
  node: true,
  text: true,
  error: true,
  'tts-sound': true,
  'drag-target': true,
  'html-tag': true,
  'script': true,
  'img': true,
};

function makeCompTree() {
  let scheme: IComps = JSON.parse(fs.readFileSync('./convert-old-xml2tsx/dom.json', 'utf-8'));

  scheme.root = scheme.types['tag'];
  for (var t in scheme.types) {
    const act = scheme.types[t];
    const typ = t == 'body' ? 'page' : t;
    if (typ[0] == '_' || typ.indexOf('doc-') == 0 || ignores[typ]) continue;
    act.typ = typ;
    if (act.anc) {
      const anc = scheme.types[act.anc];
      if (!anc.descs) anc.descs = [];
      anc.descs.push(act);
    }
    for (var p in act) {
      if (p != 'typ' && p != 'descs') delete act[p];
    }
  }

  delete scheme.types;

  fs.writeFileSync('./convert-old-xml2tsx/dom-tree.json', JSON.stringify(scheme.root, null, 2));
}

function getTS() {

  const out: Array<string> = [];
  out.push(`
import * as React from 'react';
import * as dom from './dom';

export class Tag<P extends dom.ITagProps> extends React.Component<P, any> {
  render(): JSX.Element {
    switch (React.Children.count(this.props.children)) {
      case 0: return null;
      default: return <span>{this.props.children}</span>;
    }
  }
}

export class Img extends Tag<dom.IImgProps> { }
registerTag('Img', Img);

export class DocExample extends Tag<dom.IDocExampleProps> { }
registerTag('DocExample', DocExample);

export class DocDescr extends Tag<dom.ITagProps> { }
registerTag('DocDescr', DocDescr);
`);
  const upperFirst = (s: string) => s.charAt(0).toUpperCase() + s.substr(1);

  const getComp = (comp: IComp, anc: IComp) => {
    
    if (anc) {
      const n = upperFirst(_.camelCase(comp.typ)); const a = upperFirst(_.camelCase(anc.typ));
      if (comp.descs) out.push(`export class ${n}<P extends dom.I${n}Props> extends ${a}<P> { }`);
      else out.push(`export class ${n} extends ${a}<dom.I${n}Props> { }`);
      out.push(`registerTag('${n}', ${n});`);
    }
    if (!comp.descs) return;
    comp.descs.forEach(d => getComp(d, comp));
  }

  let root: IComp = JSON.parse(fs.readFileSync('./convert-old-xml2tsx/dom-tree.json', 'utf8'));
  getComp(root, null);

  const res = out.join('\r');
  fs.writeFileSync('./convert-old-xml2tsx/comps.tsx', res);
}

makeCompTree();
getTS();