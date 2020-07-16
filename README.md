# *Engage!*

*Engage!* is an event-based parser generator.
As far as [I](http://grammarware.github.io) know, this is the only event-based parser generator in existence
(please [tell me](mailto:vadim@grammarware.net) if I'm wrong, would be glad to hear of similar projects),
even though event-based parsers like [SAX](https://en.wikipedia.org/wiki/Simple_API_for_XML) or
[RxParse](https://github.com/yongjhih/RxParse) are not really rarities. It was designed and developed
as an experiment and published at the [REBLS](https://2019.splashcon.org/home/rebls-2019) workshop at SPLASH 2019.
The [PDF of the paper](http://grammarware.net/text/2019/event-based.pdf) is freely available, it contains a
more detailed and precise description of the idea, some implementation details and empirical comparison of parsers.
If you want to cite it, feel free to use this:

```
@inproceedings{Event-Based2019,
	author    = "Vadim Zaytsev",
	title     = "{Event-Based Parsing}",
	booktitle = "{Proceedings of the Sixth Workshop on Reactive and Event-based Languages and Systems (REBLS)}",
	year      = 2019,
	editor    = "Tetsuo Kamina and Hidehiko Masuhara",
	doi       = "10.1145/3358503.3361275",
}
```

Contributors:
* **Vadim Zaytsev** aka [@grammarware](https://github.com/grammarware) — the original idea and implementation
* **Mohammed Samy** aka [@samiz](https://github.com/samiz) — a [Takmela](https://github.com/samiz/takmela)-based parser making the open-source implementation complete

This generator is open source, distributed with an [MIT license](LICENSE.md).
