.PHONY: all build build-v clean view plantuml help check-deps

MAIN_FILE = main
OUTPUT_DIR = build

help:
	@echo "Komande:"
	@echo "  make all       - Generiši PNG + PDF"
	@echo "  make build     - Samo PDF"
	@echo "  make plantuml  - Samo PNG"
	@echo "  make view      - Otvori PDF"
	@echo "  make clean     - Obriši sve"

check-deps:
	@command -v pdflatex >/dev/null 2>&1 && echo "✓ pdflatex" || echo "✗ pdflatex"
	@command -v plantuml >/dev/null 2>&1 && echo "✓ plantuml" || echo "✗ plantuml"

build:
	@mkdir -p $(OUTPUT_DIR)
	pdflatex -interaction=nonstopmode -output-directory=$(OUTPUT_DIR) $(MAIN_FILE).tex > /dev/null
	@echo "✓ PDF: $(OUTPUT_DIR)/$(MAIN_FILE).pdf"

build-v:
	@mkdir -p $(OUTPUT_DIR)
	pdflatex -interaction=nonstopmode -output-directory=$(OUTPUT_DIR) $(MAIN_FILE).tex

plantuml:
	@mkdir -p figures
	@if [ -f ../uml/02-class-diagram.puml ]; then \
		plantuml -png ../uml/02-class-diagram.puml; \
		mv ../uml/animal-shelter-class-diagram.png figures/02-class-diagram.png; \
		echo "✓ PNG: figures/animal-shelter-class-diagram.png"; \
	else \
		echo "⚠ Dijagram nije pronađen: ../uml/02-class-diagram.puml"; \
	fi

all: plantuml build
	@echo "✓ GOTOVO!"

view: build
	@if command -v xdg-open >/dev/null 2>&1; then \
		xdg-open $(OUTPUT_DIR)/$(MAIN_FILE).pdf; \
	elif command -v open >/dev/null 2>&1; then \
		open $(OUTPUT_DIR)/$(MAIN_FILE).pdf; \
	else \
		echo "Otvori ručno: $(OUTPUT_DIR)/$(MAIN_FILE).pdf"; \
	fi

clean:
	rm -rf $(OUTPUT_DIR)
	rm -f *.pdf *.aux *.log *.out *.toc *.lof *.bbl *.blg
	@echo "Done"
