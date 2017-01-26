
import * as React from 'react';
import * as dom from './dom';

declare var registerTag;

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
export class SmartTag extends Tag<dom.ISmartTagProps> { }
registerTag('SmartTag', SmartTag);
export class HeaderProp extends Tag<dom.IHeaderPropProps> { }
registerTag('HeaderProp', HeaderProp);
export class EvalControl<P extends dom.IEvalControlProps> extends Tag<P> { }
registerTag('EvalControl', EvalControl);
export class EvalButton extends EvalControl<dom.IEvalButtonProps> { }
registerTag('EvalButton', EvalButton);
export class CheckLow<P extends dom.ICheckLowProps> extends EvalControl<P> { }
registerTag('CheckLow', CheckLow);
export class CheckBox extends CheckLow<dom.ICheckBoxProps> { }
registerTag('CheckBox', CheckBox);
export class CheckItem extends CheckLow<dom.ICheckItemProps> { }
registerTag('CheckItem', CheckItem);
export class RadioButton extends EvalControl<dom.IRadioButtonProps> { }
registerTag('RadioButton', RadioButton);
export class WordSelection extends EvalControl<dom.IWordSelectionProps> { }
registerTag('WordSelection', WordSelection);
export class WordMultiSelection extends EvalControl<dom.IWordMultiSelectionProps> { }
registerTag('WordMultiSelection', WordMultiSelection);
export class WordOrdering extends EvalControl<dom.IWordOrderingProps> { }
registerTag('WordOrdering', WordOrdering);
export class SentenceOrdering extends EvalControl<dom.ISentenceOrderingProps> { }
registerTag('SentenceOrdering', SentenceOrdering);
export class Edit<P extends dom.IEditProps> extends EvalControl<P> { }
registerTag('Edit', Edit);
export class GapFill extends Edit<dom.IGapFillProps> { }
registerTag('GapFill', GapFill);
export class DropDown extends Edit<dom.IDropDownProps> { }
registerTag('DropDown', DropDown);
export class Pairing extends EvalControl<dom.IPairingProps> { }
registerTag('Pairing', Pairing);
export class HumanEval<P extends dom.IHumanEvalProps> extends EvalControl<P> { }
registerTag('HumanEval', HumanEval);
export class Writing extends HumanEval<dom.IWritingProps> { }
registerTag('Writing', Writing);
export class Recording extends HumanEval<dom.IRecordingProps> { }
registerTag('Recording', Recording);
export class Extension extends EvalControl<dom.IExtensionProps> { }
registerTag('Extension', Extension);
export class Page extends Tag<dom.IPageProps> { }
registerTag('Page', Page);
export class Offering extends Tag<dom.IOfferingProps> { }
registerTag('Offering', Offering);
export class SingleChoice extends Tag<dom.ISingleChoiceProps> { }
registerTag('SingleChoice', SingleChoice);
export class SentenceOrderingItem extends Tag<dom.ISentenceOrderingItemProps> { }
registerTag('SentenceOrderingItem', SentenceOrderingItem);
export class PairingItem extends Tag<dom.IPairingItemProps> { }
registerTag('PairingItem', PairingItem);
export class Macro<P extends dom.IMacroProps> extends Tag<P> { }
registerTag('Macro', Macro);
export class List extends Macro<dom.IListProps> { }
registerTag('List', List);
export class ListGroup extends Macro<dom.IListGroupProps> { }
registerTag('ListGroup', ListGroup);
export class TwoColumn extends Macro<dom.ITwoColumnProps> { }
registerTag('TwoColumn', TwoColumn);
export class Panel extends Macro<dom.IPanelProps> { }
registerTag('Panel', Panel);
export class MacroTemplate<P extends dom.IMacroTemplateProps> extends Macro<P> { }
registerTag('MacroTemplate', MacroTemplate);
export class MacroArticle extends MacroTemplate<dom.IMacroArticleProps> { }
registerTag('MacroArticle', MacroArticle);
export class MacroVocabulary extends MacroTemplate<dom.IMacroVocabularyProps> { }
registerTag('MacroVocabulary', MacroVocabulary);
export class MacroVideo extends MacroTemplate<dom.IMacroVideoProps> { }
registerTag('MacroVideo', MacroVideo);
export class MacroTrueFalse extends MacroTemplate<dom.IMacroTrueFalseProps> { }
registerTag('MacroTrueFalse', MacroTrueFalse);
export class MacroSingleChoices extends MacroTemplate<dom.IMacroSingleChoicesProps> { }
registerTag('MacroSingleChoices', MacroSingleChoices);
export class MacroListWordOrdering extends MacroTemplate<dom.IMacroListWordOrderingProps> { }
registerTag('MacroListWordOrdering', MacroListWordOrdering);
export class MacroPairing extends MacroTemplate<dom.IMacroPairingProps> { }
registerTag('MacroPairing', MacroPairing);
export class MacroTable extends MacroTemplate<dom.IMacroTableProps> { }
registerTag('MacroTable', MacroTable);
export class MacroList extends MacroTemplate<dom.IMacroListProps> { }
registerTag('MacroList', MacroList);
export class MacroIconList extends MacroTemplate<dom.IMacroIconListProps> { }
registerTag('MacroIconList', MacroIconList);
export class SmartElementLow<P extends dom.ISmartElementLowProps> extends MacroTemplate<P> { }
registerTag('SmartElementLow', SmartElementLow);
export class SmartElement extends SmartElementLow<dom.ISmartElementProps> { }
registerTag('SmartElement', SmartElement);
export class SmartPairing extends SmartElementLow<dom.ISmartPairingProps> { }
registerTag('SmartPairing', SmartPairing);
export class SmartOffering extends SmartElementLow<dom.ISmartOfferingProps> { }
registerTag('SmartOffering', SmartOffering);
export class InlineTag extends MacroTemplate<dom.IInlineTagProps> { }
registerTag('InlineTag', InlineTag);
export class Include<P extends dom.IIncludeProps> extends Tag<P> { }
registerTag('Include', Include);
export class IncludeText extends Include<dom.IIncludeTextProps> { }
registerTag('IncludeText', IncludeText);
export class IncludeDialog extends Include<dom.IIncludeDialogProps> { }
registerTag('IncludeDialog', IncludeDialog);
export class PhraseReplace extends Tag<dom.IPhraseReplaceProps> { }
registerTag('PhraseReplace', PhraseReplace);
export class Phrase extends Tag<dom.IPhraseProps> { }
registerTag('Phrase', Phrase);
export class Replica extends Tag<dom.IReplicaProps> { }
registerTag('Replica', Replica);
export class UrlTag<P extends dom.IUrlTagProps> extends Tag<P> { }
registerTag('UrlTag', UrlTag);
export class MediaTag<P extends dom.IMediaTagProps> extends UrlTag<P> { }
registerTag('MediaTag', MediaTag);
export class MediaText extends MediaTag<dom.IMediaTextProps> { }
registerTag('MediaText', MediaText);
export class MediaBigMark extends MediaTag<dom.IMediaBigMarkProps> { }
registerTag('MediaBigMark', MediaBigMark);
export class MediaPlayer extends MediaTag<dom.IMediaPlayerProps> { }
registerTag('MediaPlayer', MediaPlayer);
export class MediaVideo extends MediaTag<dom.IMediaVideoProps> { }
registerTag('MediaVideo', MediaVideo);